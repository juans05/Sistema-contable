using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class PlanContableRepository : IPlanContableRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<PlanContableRepository> _logger;

        public PlanContableRepository(NpgsqlDataSource dataSource, ILogger<PlanContableRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<EPlanContableGeneral>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                SELECT p.id, p.empresa_id as EmpresaId, p.cuenta, p.descripcion, p.nivel, p.elemento, p.cta, p.clase_cuenta AS claseCuenta, 
                       p.tipo_anexo AS tipoAnexo, p.cuenta_monetaria AS cuentaMonetaria, p.ajuste_dif_cambio AS ajusteDifCambio, 
                       p.requiere_centro_costo AS requiereCentroCosto, p.codigo_eeff_estand AS codigoEeffEstand, 
                       p.codigo_eeff_trib AS codigoEeffTrib, p.clasificacion_bien_serv AS clasificacionBienServ, 
                       p.cargo_1 AS cargo1, p.abono_1 AS abono1, p.porcentaje_1 AS porcentaje1, p.cuenta_cierre AS cuentaCierre, p.activo 
                FROM plan_contable p
                INNER JOIN empresas e ON p.empresa_id = e.id
                WHERE e.ruc = @rucEmpresa AND (@activo IS NULL OR p.activo = @activo) 
                ORDER BY p.cuenta";
            var result = await connection.QueryAsync<EPlanContableGeneral>(query, new { rucEmpresa, activo });
            return result.ToList();
        }

        public async Task<EPlanContableGeneral?> ObtenerPorIdAsync(string rucEmpresa, int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                SELECT p.id, p.empresa_id as EmpresaId, p.cuenta, p.descripcion, p.nivel, p.elemento, p.cta, p.clase_cuenta AS claseCuenta, 
                       p.tipo_anexo AS tipoAnexo, p.cuenta_monetaria AS cuentaMonetaria, p.ajuste_dif_cambio AS ajusteDifCambio, 
                       p.requiere_centro_costo AS requiereCentroCosto, p.codigo_eeff_estand AS codigoEeffEstand, 
                       p.codigo_eeff_trib AS codigoEeffTrib, p.clasificacion_bien_serv AS clasificacionBienServ, 
                       p.cargo_1 AS cargo1, p.abono_1 AS abono1, p.porcentaje_1 AS porcentaje1, p.cuenta_cierre AS cuentaCierre, p.activo 
                FROM plan_contable p
                INNER JOIN empresas e ON p.empresa_id = e.id
                WHERE e.ruc = @rucEmpresa AND p.id = @id";
            return await connection.QuerySingleOrDefaultAsync<EPlanContableGeneral>(query, new { rucEmpresa, id });
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearPlanContableRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });
            if (empresaId == 0) throw new Exception("Empresa no encontrada");

            var query = @"
                INSERT INTO plan_contable (empresa_id, cuenta, descripcion, nivel, elemento, cta, clase_cuenta, 
                                           tipo_anexo, cuenta_monetaria, ajuste_dif_cambio, requiere_centro_costo, 
                                           codigo_eeff_estand, codigo_eeff_trib, clasificacion_bien_serv, 
                                           cargo_1, abono_1, porcentaje_1, cuenta_cierre, activo, created_at) 
                VALUES (@empresaId, @cuenta, @descripcion, @nivel, @elemento, @cta, @claseCuenta, 
                        @tipoAnexo, @cuentaMonetaria, @ajusteDifCambio, @requiereCentroCosto, 
                        @codigoEeffEstand, @codigoEeffTrib, @clasificacionBienServ, 
                        @cargo1, @abono1, @porcentaje1, @cuentaCierre, @activo, CURRENT_TIMESTAMP) RETURNING id";
            
            return await connection.ExecuteScalarAsync<int>(query, new 
            { 
                empresaId, request.Cuenta, request.Descripcion, request.Nivel, request.Elemento, request.Cta, 
                request.ClaseCuenta, request.TipoAnexo, request.CuentaMonetaria, request.AjusteDifCambio, 
                request.RequiereCentroCosto, request.CodigoEeffEstand, request.CodigoEeffTrib, 
                request.ClasificacionBienServ, request.Cargo1, request.Abono1, request.Porcentaje1, 
                request.CuentaCierre, request.Activo
            });
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarPlanContableRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });

            var query = @"
                UPDATE plan_contable 
                SET cuenta = @cuenta, descripcion = @descripcion, nivel = @nivel, elemento = @elemento, 
                    cta = @cta, clase_cuenta = @claseCuenta, tipo_anexo = @tipoAnexo, 
                    cuenta_monetaria = @cuentaMonetaria, ajuste_dif_cambio = @ajusteDifCambio, 
                    requiere_centro_costo = @requiereCentroCosto, codigo_eeff_estand = @codigoEeffEstand, 
                    codigo_eeff_trib = @codigoEeffTrib, clasificacion_bien_serv = @clasificacionBienServ, 
                    cargo_1 = @cargo1, abono_1 = @abono1, porcentaje_1 = @porcentaje1, cuenta_cierre = @cuentaCierre, 
                    activo = @activo, updated_at = CURRENT_TIMESTAMP
                WHERE id = @id AND empresa_id = @empresaId";
            
            var records = await connection.ExecuteAsync(query, new
            {
                id, empresaId, request.Cuenta, request.Descripcion, request.Nivel, request.Elemento, request.Cta, 
                request.ClaseCuenta, request.TipoAnexo, request.CuentaMonetaria, request.AjusteDifCambio, 
                request.RequiereCentroCosto, request.CodigoEeffEstand, request.CodigoEeffTrib, 
                request.ClasificacionBienServ, request.Cargo1, request.Abono1, request.Porcentaje1, 
                request.CuentaCierre, request.Activo
            });
            return records > 0;
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });
            var query = "DELETE FROM plan_contable WHERE id = @id AND empresa_id = @empresaId";
            var records = await connection.ExecuteAsync(query, new { id, empresaId });
            return records > 0;
        }
    }
}