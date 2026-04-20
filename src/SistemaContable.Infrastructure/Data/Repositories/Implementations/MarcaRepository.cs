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
    public class MarcaRepository : IMarcaRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<MarcaRepository> _logger;

        public MarcaRepository(NpgsqlDataSource dataSource, ILogger<MarcaRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<EMarca>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var query = @"
                    SELECT m.id, m.empresa_id as EmpresaId, m.codigo, m.nombre, m.descripcion, m.origen, m.activo 
                    FROM marcas m
                    INNER JOIN empresas e ON m.empresa_id = e.id
                    WHERE e.ruc = @rucEmpresa 
                      AND (@activo IS NULL OR m.activo = @activo) 
                    ORDER BY m.nombre";
                var result = await connection.QueryAsync<EMarca>(query, new { rucEmpresa, activo });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar marcas");
                throw;
            }
        }

        public async Task<EMarca?> ObtenerPorIdAsync(string rucEmpresa, int idMarca)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var query = @"
                    SELECT m.id, m.empresa_id as EmpresaId, m.codigo, m.nombre, m.descripcion, m.origen, m.activo 
                    FROM marcas m
                    INNER JOIN empresas e ON m.empresa_id = e.id
                    WHERE e.ruc = @rucEmpresa AND m.id = @idMarca";
                return await connection.QuerySingleOrDefaultAsync<EMarca>(query, new { rucEmpresa, idMarca });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marca {Id}", idMarca);
                throw;
            }
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearMarcaRequest request)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                if (empresaId == 0) throw new Exception("Empresa no encontrada");

                var query = @"
                    INSERT INTO marcas (empresa_id, codigo, nombre, descripcion, origen, activo, created_at) 
                    VALUES (@empresaId, @codigo, @nombre, @descripcion, @origen, @activo, CURRENT_TIMESTAMP) RETURNING id";

                return await connection.ExecuteScalarAsync<int>(query, new 
                { 
                    empresaId,
                    codigo = request.Codigo,
                    nombre = request.Nombre, 
                    descripcion = request.Descripcion,
                    origen = request.Origen,
                    activo = request.Activo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear marca");
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int idMarca, ActualizarMarcaRequest request)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                var query = @"
                    UPDATE marcas 
                    SET codigo = @codigo, nombre = @nombre, descripcion = @descripcion, origen = @origen, activo = @activo, updated_at = CURRENT_TIMESTAMP
                    WHERE id = @idMarca AND empresa_id = @empresaId";

                var records = await connection.ExecuteAsync(query, new
                {
                    codigo = request.Codigo,
                    nombre = request.Nombre,
                    descripcion = request.Descripcion,
                    origen = request.Origen,
                    activo = request.Activo,
                    idMarca,
                    empresaId
                });
                return records > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar marca {Id}", idMarca);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int idMarca)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                var queryV = "SELECT COUNT(*) FROM productos WHERE marca_id = @idMarca";
                var productosCount = await connection.ExecuteScalarAsync<int>(queryV, new { idMarca });
                if (productosCount > 0)
                {
                    throw new InvalidOperationException("No se puede eliminar la marca porque está asignada a uno o más productos.");
                }

                var query = "DELETE FROM marcas WHERE id = @idMarca AND empresa_id = @empresaId";
                var records = await connection.ExecuteAsync(query, new { idMarca, empresaId });
                return records > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar marca {Id}", idMarca);
                throw;
            }
        }
    }
}