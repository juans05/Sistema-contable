using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class AccountingRepository : IAccountingRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<AccountingRepository> _logger;

        public AccountingRepository(IConfiguration configuration, ILogger<AccountingRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string no configurada");
            _logger = logger;
        }

        public async Task<List<EReglaContable>> ObtenerReglasPorEventoAsync(string codigoEvento, int empresaId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                // Busca reglas específicas de la empresa O reglas genéricas (empresa_id NULL)
                // Ordenadas por 'orden'
                var sql = @"
                    SELECT 
                        r.id, r.evento_tipo_id AS EventoTipoId, r.orden,
                        r.cuenta_codigo_base AS CuentaCodigoBase,
                        r.cuenta_dinamica_tipo AS CuentaDinamicaTipo,
                        r.naturaleza,
                        r.formula_monto AS FormulaMonto,
                        r.glosa_plantilla AS GlosaPlantilla,
                        r.condicion_sql AS CondicionSql,
                        r.empresa_id AS EmpresaId,
                        r.activo
                    FROM ""suizaConta"".contabilidad_reglas r
                    INNER JOIN ""suizaConta"".contabilidad_eventos_tipo e ON e.id = r.evento_tipo_id
                    WHERE e.codigo_evento = @CodigoEvento
                      AND r.activo = TRUE
                      AND (r.empresa_id = @EmpresaId OR r.empresa_id IS NULL)
                    ORDER BY r.orden ASC";

                var result = await connection.QueryAsync<EReglaContable>(sql, new { CodigoEvento = codigoEvento, EmpresaId = empresaId });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo reglas contables para {Evento}", codigoEvento);
                throw;
            }
        }

        public async Task<EPlanContable> ObtenerCuentaPorCodigoAsync(string codigo, int empresaId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                var sql = @"
                    SELECT 
                        id, codigo, nombre, nivel, tipo_cuenta AS TipoCuenta,
                        moneda, analisis, permite_movimiento AS PermiteMovimiento, activo
                    FROM ""suizaConta"".contabilidad_plan_cuentas
                    WHERE codigo = @Codigo 
                      AND (empresa_id = @EmpresaId OR empresa_id IS NULL)
                    LIMIT 1";

                return await connection.QueryFirstOrDefaultAsync<EPlanContable>(sql, new { Codigo = codigo, EmpresaId = empresaId });
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error buscando cuenta contable {Codigo}", codigo);
                return null;
            }
        }

        public async Task<int> GuardarAsientoCompletoAsync(EAsientoContable asiento)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // 1. Insertar Cabecera
                var sqlCabecera = @"
                    INSERT INTO ""suizaConta"".contabilidad_asientos 
                    (empresa_id, periodo, fecha_contable, glosa, origen_modulo, origen_id_referencia, codigo_unico_operacion, moneda, tipo_cambio, estado, usuario_creacion)
                    VALUES 
                    (@EmpresaId, @Periodo, @FechaContable, @Glosa, @OrigenModulo, @OrigenIdReferencia, @CodigoUnicoOperacion, @Moneda, @TipoCambio, @Estado, @UsuarioCreacion)
                    RETURNING id";

                var asientoId = await connection.ExecuteScalarAsync<int>(sqlCabecera, asiento, transaction);
                asiento.Id = asientoId;

                // 2. Insertar Detalles
                var sqlDetalle = @"
                    INSERT INTO ""suizaConta"".contabilidad_asientos_detalle
                    (asiento_id, cuenta_codigo, descripcion_cuenta, debe_origen, haber_origen, debe_pen, haber_pen, debe_usd, haber_usd, orden)
                    VALUES 
                    (@AsientoId, @CuentaCodigo, @DescripcionCuenta, @DebeOrigen, @HaberOrigen, @DebePen, @HaberPen, @DebeUsd, @HaberUsd, @Orden)";

                foreach (var det in asiento.Detalles)
                {
                    det.AsientoId = asientoId;
                    await connection.ExecuteAsync(sqlDetalle, det, transaction);
                }

                await transaction.CommitAsync();
                return asientoId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error guardando asiento contable transacción");
                throw;
            }
        }
        public async Task<string> ObtenerConfiguracionAsync(string clave, int empresaId)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                var sql = @"
                    SELECT valor 
                    FROM ""suizaConta"".contabilidad_configuracion
                    WHERE empresa_id = @EmpresaId AND clave = @Clave";
                
                return await connection.ExecuteScalarAsync<string>(sql, new { EmpresaId = empresaId, Clave = clave });
            }
            catch (Exception ex)
            {
                // No loguear error, puede ser algo frecuente no encontrar config
                return null;
            }
        }
        public async Task<List<EEventoContableTipo>> ListarEventosDisponiblesAsync()
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"SELECT id, codigo_evento AS CodigoEvento, descripcion, modulo_origen AS ModuloOrigen 
                        FROM ""suizaConta"".contabilidad_eventos_tipo ORDER BY descripcion";
            var result = await connection.QueryAsync<EEventoContableTipo>(sql);
            return result.ToList();
        }

        public async Task<bool> GuardarReglaAsync(EReglaContable regla)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            string sql;
            
            if (regla.Id == 0)
            {
                sql = @"INSERT INTO ""suizaConta"".contabilidad_reglas 
                        (evento_tipo_id, orden, cuenta_codigo_base, cuenta_dinamica_tipo, naturaleza, formula_monto, glosa_plantilla, empresa_id, activo)
                        VALUES 
                        (@EventoTipoId, @Orden, @CuentaCodigoBase, @CuentaDinamicaTipo, @Naturaleza, @FormulaMonto, @GlosaPlantilla, @EmpresaId, TRUE)";
            }
            else
            {
                sql = @"UPDATE ""suizaConta"".contabilidad_reglas SET
                        evento_tipo_id = @EventoTipoId,
                        orden = @Orden,
                        cuenta_codigo_base = @CuentaCodigoBase,
                        naturaleza = @Naturaleza,
                        formula_monto = @FormulaMonto,
                        glosa_plantilla = @GlosaPlantilla
                        WHERE id = @Id AND (empresa_id = @EmpresaId OR empresa_id IS NULL)";
            }
            
            var rows = await connection.ExecuteAsync(sql, regla);
            return rows > 0;
        }

        public async Task<bool> EliminarReglaAsync(int id, int empresaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            // Solo borrado lógico
            var sql = @"UPDATE ""suizaConta"".contabilidad_reglas SET activo = FALSE 
                        WHERE id = @Id AND (empresa_id = @EmpresaId OR empresa_id IS NULL)";
            var rows = await connection.ExecuteAsync(sql, new { Id = id, EmpresaId = empresaId });
            return rows > 0;
        }

        public async Task<bool> ImportarPlanCuentasAsync(List<EPlanContable> cuentas, int empresaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Estrategia: Upsert (Insertar o Actualizar si existe codigo+empresa)
                // Usamos PostgreSQL ON CONFLICT (codigo, empresa_id) DO UPDATE
                // PERO, la tabla actual puede no tener unique constraint en (codigo, empresa_id) o permitir nulos en empresa_id.
                // Asumiremos que para esta empresa, el código debe ser único.
                
                // Primero borramos las existentes para esta empresa (Opcional: o hacemos upsert uno a uno).
                // Para ser menos destructivos y permitir actualizaciones de nombres, haremos UPSERT loop.
                // Requerimos que exista un Unique Index en (codigo, empresa_id).
                // Si no existe, lo hacemos "Delete all for company -> Insert" o "Check exist -> Update/Insert".
                // Dado que no puedo alterar esquema fácilmente ahora, usaré "Check Exists" o DELETE previo si el usuario quiere reemplazo total.
                // Por seguridad: DELETE previo de lo que se va a cargar NO es buena idea si hay FKs.
                // Haremos INSERT ... ON CONFLICT (codigo, empresa_id) si existe constraint. 
                // Si no, lo hacemos manual.
                
                var sqlCheck = @"SELECT id FROM ""suizaConta"".contabilidad_plan_cuentas WHERE codigo = @Codigo AND empresa_id = @EmpresaId";
                var sqlUpdate = @"UPDATE ""suizaConta"".contabilidad_plan_cuentas SET nombre = @Nombre, nivel = @Nivel, tipo_cuenta = @TipoCuenta, moneda = @Moneda, analisis = @Analisis, permite_movimiento = @PermiteMovimiento, activo = TRUE WHERE id = @Id";
                var sqlInsert = @"INSERT INTO ""suizaConta"".contabilidad_plan_cuentas (codigo, nombre, nivel, tipo_cuenta, moneda, analisis, permite_movimiento, empresa_id, activo) VALUES (@Codigo, @Nombre, @Nivel, @TipoCuenta, @Moneda, @Analisis, @PermiteMovimiento, @EmpresaId, TRUE)";

                int inserted = 0;
                int updated = 0;

                foreach (var c in cuentas)
                {
                    c.EmpresaId = empresaId;
                    var existingId = await connection.ExecuteScalarAsync<int?>(sqlCheck, new { c.Codigo, EmpresaId = empresaId }, transaction);

                    if (existingId.HasValue)
                    {
                        c.Id = existingId.Value;
                        await connection.ExecuteAsync(sqlUpdate, c, transaction);
                        updated++;
                    }
                    else
                    {
                        await connection.ExecuteAsync(sqlInsert, c, transaction);
                        inserted++;
                    }
                }

                await transaction.CommitAsync();
                _logger.LogInformation($"Importación Plan Cuentas Completa: Empresa {empresaId}. Insertados: {inserted}, Actualizados: {updated}");
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error importando plan de cuentas");
                return false; 
            }
        }

        public async Task<List<EPlanContable>> ListarPlanCuentasAsync(int empresaId, string busqueda = null)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            var sql = @"
                SELECT 
                    id, codigo, nombre, nivel, tipo_cuenta AS TipoCuenta,
                    moneda, analisis, permite_movimiento AS PermiteMovimiento, activo, empresa_id AS EmpresaId
                FROM ""suizaConta"".contabilidad_plan_cuentas
                WHERE (empresa_id = @EmpresaId OR empresa_id IS NULL)
                  AND activo = TRUE";

            if (!string.IsNullOrEmpty(busqueda))
            {
                sql += " AND (codigo ILIKE @Busqueda OR nombre ILIKE @Busqueda)";
            }

            sql += " ORDER BY codigo";

            var result = (await connection.QueryAsync<EPlanContable>(sql, new { EmpresaId = empresaId, Busqueda = $"%{busqueda}%" })).ToList();
            _logger.LogInformation($"ListarPlanCuentas: Empresa {empresaId}, Busqueda '{busqueda}', Encontrados: {result.Count}");
            return result;
        }

        public async Task<bool> GuardarCuentaAsync(EPlanContable cuenta)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            string sql;
            
            // Validar unicidad de código para la empresa si es nuevo
            if (cuenta.Id == 0)
            {
                 var exists = await connection.ExecuteScalarAsync<int>(
                    @"SELECT COUNT(1) FROM ""suizaConta"".contabilidad_plan_cuentas 
                      WHERE codigo = @Codigo AND (empresa_id = @EmpresaId OR empresa_id IS NULL) AND activo = TRUE", 
                    new { cuenta.Codigo, cuenta.EmpresaId });
                 
                 if (exists > 0) throw new InvalidOperationException($"El código {cuenta.Codigo} ya existe.");

                 sql = @"INSERT INTO ""suizaConta"".contabilidad_plan_cuentas 
                        (codigo, nombre, nivel, tipo_cuenta, moneda, analisis, permite_movimiento, empresa_id, activo)
                        VALUES 
                        (@Codigo, @Nombre, @Nivel, @TipoCuenta, @Moneda, @Analisis, @PermiteMovimiento, @EmpresaId, TRUE)";
            }
            else
            {
                 sql = @"UPDATE ""suizaConta"".contabilidad_plan_cuentas SET
                        nombre = @Nombre, nivel = @Nivel, tipo_cuenta = @TipoCuenta, 
                        moneda = @Moneda, analisis = @Analisis, permite_movimiento = @PermiteMovimiento
                        WHERE id = @Id AND empresa_id = @EmpresaId";
            }

            var rows = await connection.ExecuteAsync(sql, cuenta);
            return rows > 0;
        }

        public async Task<bool> EliminarCuentaAsync(int id, int empresaId)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            // Soft delete
            var sql = @"UPDATE ""suizaConta"".contabilidad_plan_cuentas SET activo = FALSE 
                        WHERE id = @Id AND empresa_id = @EmpresaId";
            var rows = await connection.ExecuteAsync(sql, new { Id = id, EmpresaId = empresaId });
            return rows > 0;
        }
    }
}
