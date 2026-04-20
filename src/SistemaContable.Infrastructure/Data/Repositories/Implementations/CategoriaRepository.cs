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
    public class CategoriaRepository : ICategoriaRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<CategoriaRepository> _logger;

        public CategoriaRepository(NpgsqlDataSource dataSource, ILogger<CategoriaRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<List<ECategoria>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var query = @"
                    SELECT c.id, c.empresa_id as EmpresaId, c.codigo, c.nombre, c.descripcion, c.categoria_padre_id AS categoriaPadreId, c.activo 
                    FROM categorias c
                    INNER JOIN empresas e ON c.empresa_id = e.id
                    WHERE e.ruc = @rucEmpresa 
                      AND (@activo IS NULL OR c.activo = @activo) 
                    ORDER BY c.nombre";
                var result = await connection.QueryAsync<ECategoria>(query, new { rucEmpresa, activo });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar categorías");
                throw;
            }
        }

        public async Task<ECategoria?> ObtenerPorIdAsync(string rucEmpresa, int idCategoria)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var query = @"
                    SELECT c.id, c.empresa_id as EmpresaId, c.codigo, c.nombre, c.descripcion, c.categoria_padre_id AS categoriaPadreId, c.activo 
                    FROM categorias c
                    INNER JOIN empresas e ON c.empresa_id = e.id
                    WHERE e.ruc = @rucEmpresa AND c.id = @idCategoria";
                return await connection.QuerySingleOrDefaultAsync<ECategoria>(query, new { rucEmpresa, idCategoria });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría {Id}", idCategoria);
                throw;
            }
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearCategoriaRequest request)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                if (empresaId == 0) throw new Exception("Empresa no encontrada");

                var query = @"
                    INSERT INTO categorias (empresa_id, codigo, nombre, descripcion, categoria_padre_id, activo, created_at) 
                    VALUES (@empresaId, @codigo, @nombre, @descripcion, @categoriaPadreId, @activo, CURRENT_TIMESTAMP) RETURNING id";

                return await connection.ExecuteScalarAsync<int>(query, new 
                { 
                    empresaId,
                    codigo = request.Codigo,
                    nombre = request.Nombre, 
                    descripcion = request.Descripcion,
                    categoriaPadreId = request.CategoriaPadreId,
                    activo = request.Activo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int idCategoria, ActualizarCategoriaRequest request)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                var query = @"
                    UPDATE categorias 
                    SET codigo = @codigo, nombre = @nombre, descripcion = @descripcion, categoria_padre_id = @categoriaPadreId, activo = @activo, updated_at = CURRENT_TIMESTAMP
                    WHERE id = @idCategoria AND empresa_id = @empresaId";

                var records = await connection.ExecuteAsync(query, new
                {
                    codigo = request.Codigo,
                    nombre = request.Nombre,
                    descripcion = request.Descripcion,
                    categoriaPadreId = request.CategoriaPadreId,
                    activo = request.Activo,
                    idCategoria,
                    empresaId
                });
                return records > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría {Id}", idCategoria);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int idCategoria)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var queryEmpresa = "SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1";
                var empresaId = await connection.ExecuteScalarAsync<int>(queryEmpresa, new { rucEmpresa });

                // Validación simple para no romper llaves foráneas con productos
                var queryV = "SELECT COUNT(*) FROM productos WHERE categoria_id = @idCategoria";
                var productosCount = await connection.ExecuteScalarAsync<int>(queryV, new { idCategoria });
                if (productosCount > 0)
                {
                    throw new InvalidOperationException("No se puede eliminar la categoría porque está asignada a uno o más productos.");
                }

                var query = "DELETE FROM categorias WHERE id = @idCategoria AND empresa_id = @empresaId";
                var records = await connection.ExecuteAsync(query, new { idCategoria, empresaId });
                return records > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría {Id}", idCategoria);
                throw;
            }
        }
    }
}