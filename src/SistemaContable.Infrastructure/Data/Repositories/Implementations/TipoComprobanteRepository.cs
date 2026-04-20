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
    public class TipoComprobanteRepository : ITipoComprobanteRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<TipoComprobanteRepository> _logger;

        public TipoComprobanteRepository(NpgsqlDataSource dataSource, ILogger<TipoComprobanteRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<ETipoComprobante>> ListarAsync(bool? activo = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = "SELECT id, codigo, descripcion, codigo_sunat AS codigoSunat, tipo, activo FROM tipos_comprobante WHERE (@activo IS NULL OR activo = @activo) ORDER BY codigo";
            var result = await connection.QueryAsync<ETipoComprobante>(query, new { activo });
            return result.ToList();
        }

        public async Task<ETipoComprobante?> ObtenerPorIdAsync(int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = "SELECT id, codigo, descripcion, codigo_sunat AS codigoSunat, tipo, activo FROM tipos_comprobante WHERE id = @id";
            return await connection.QuerySingleOrDefaultAsync<ETipoComprobante>(query, new { id });
        }

        public async Task<int> CrearAsync(CrearTipoComprobanteRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                INSERT INTO tipos_comprobante (codigo, descripcion, codigo_sunat, tipo, activo, created_at) 
                VALUES (@codigo, @descripcion, @codigoSunat, @tipo, @activo, CURRENT_TIMESTAMP) RETURNING id";
            return await connection.ExecuteScalarAsync<int>(query, new 
            { 
                codigo = request.Codigo, descripcion = request.Descripcion, 
                codigoSunat = request.CodigoSunat, tipo = request.Tipo, activo = request.Activo 
            });
        }

        public async Task<bool> ActualizarAsync(int id, ActualizarTipoComprobanteRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                UPDATE tipos_comprobante SET codigo = @codigo, descripcion = @descripcion, 
                codigo_sunat = @codigoSunat, tipo = @tipo, activo = @activo
                WHERE id = @id";
            var records = await connection.ExecuteAsync(query, new 
            { 
                id, codigo = request.Codigo, descripcion = request.Descripcion, 
                codigoSunat = request.CodigoSunat, tipo = request.Tipo, activo = request.Activo 
            });
            return records > 0;
        }

        public async Task<bool> EliminarAsync(int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = "DELETE FROM tipos_comprobante WHERE id = @id";
            var records = await connection.ExecuteAsync(query, new { id });
            return records > 0;
        }
    }
}