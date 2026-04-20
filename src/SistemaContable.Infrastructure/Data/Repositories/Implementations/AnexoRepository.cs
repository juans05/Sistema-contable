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
    public class AnexoRepository : IAnexoRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<AnexoRepository> _logger;

        public AnexoRepository(NpgsqlDataSource dataSource, ILogger<AnexoRepository> logger)
        {
            _dataSource = dataSource;
            _logger = logger;
        }

        public async Task<List<EAnexo>> ListarAsync(string rucEmpresa, string? tipoAnexo = null, bool? activo = null)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                SELECT a.id, a.empresa_id as EmpresaId, a.tipo_anexo AS tipoAnexo, a.codigo_anexo AS codigoAnexo, a.tipo_documento_id AS tipoDocumentoId, 
                       a.numero_documento AS numeroDocumento, a.tipo_persona AS tipoPersona, a.razon_social AS razonSocial, 
                       a.nombres, a.apellido_paterno AS apellidoPaterno, a.apellido_materno AS apellidoMaterno, 
                       a.sexo, a.nacionalidad, a.direccion, a.correo, a.activo 
                FROM anexos a
                INNER JOIN empresas e ON a.empresa_id = e.id
                WHERE e.ruc = @rucEmpresa 
                  AND (@tipoAnexo IS NULL OR a.tipo_anexo = @tipoAnexo)
                  AND (@activo IS NULL OR a.activo = @activo) 
                ORDER BY a.razon_social, a.nombres";
            var result = await connection.QueryAsync<EAnexo>(query, new { rucEmpresa, tipoAnexo, activo });
            return result.ToList();
        }

        public async Task<EAnexo?> ObtenerPorIdAsync(string rucEmpresa, int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var query = @"
                SELECT a.id, a.empresa_id as EmpresaId, a.tipo_anexo AS tipoAnexo, a.codigo_anexo AS codigoAnexo, a.tipo_documento_id AS tipoDocumentoId, 
                       a.numero_documento AS numeroDocumento, a.tipo_persona AS tipoPersona, a.razon_social AS razonSocial, 
                       a.nombres, a.apellido_paterno AS apellidoPaterno, a.apellido_materno AS apellidoMaterno, 
                       a.sexo, a.nacionalidad, a.direccion, a.correo, a.activo 
                FROM anexos a
                INNER JOIN empresas e ON a.empresa_id = e.id
                WHERE e.ruc = @rucEmpresa AND a.id = @id";
            return await connection.QuerySingleOrDefaultAsync<EAnexo>(query, new { rucEmpresa, id });
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearAnexoRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });
            if (empresaId == 0) throw new Exception("Empresa no encontrada");

            var query = @"
                INSERT INTO anexos (empresa_id, tipo_anexo, codigo_anexo, tipo_documento_id, numero_documento, 
                                    tipo_persona, razon_social, nombres, apellido_paterno, apellido_materno, 
                                    sexo, nacionalidad, direccion, correo, activo, created_at) 
                VALUES (@empresaId, @tipoAnexo, @codigoAnexo, @tipoDocumentoId, @numeroDocumento, 
                        @tipoPersona, @razonSocial, @nombres, @apellidoPaterno, @apellidoMaterno, 
                        @sexo, @nacionalidad, @direccion, @correo, @activo, CURRENT_TIMESTAMP) RETURNING id";
            
            return await connection.ExecuteScalarAsync<int>(query, new 
            { 
                empresaId, request.TipoAnexo, request.CodigoAnexo, request.TipoDocumentoId, request.NumeroDocumento, 
                request.TipoPersona, request.RazonSocial, request.Nombres, request.ApellidoPaterno, request.ApellidoMaterno, 
                request.Sexo, request.Nacionalidad, request.Direccion, request.Correo, request.Activo
            });
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarAnexoRequest request)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });

            var query = @"
                UPDATE anexos 
                SET tipo_anexo = @tipoAnexo, codigo_anexo = @codigoAnexo, tipo_documento_id = @tipoDocumentoId, 
                    numero_documento = @numeroDocumento, tipo_persona = @tipoPersona, razon_social = @razonSocial, 
                    nombres = @nombres, apellido_paterno = @apellidoPaterno, apellido_materno = @apellidoMaterno, 
                    sexo = @sexo, nacionalidad = @nacionalidad, direccion = @direccion, correo = @correo, 
                    activo = @activo, updated_at = CURRENT_TIMESTAMP
                WHERE id = @id AND empresa_id = @empresaId";
            
            var records = await connection.ExecuteAsync(query, new
            {
                empresaId, id, request.TipoAnexo, request.CodigoAnexo, request.TipoDocumentoId, request.NumeroDocumento, 
                request.TipoPersona, request.RazonSocial, request.Nombres, request.ApellidoPaterno, request.ApellidoMaterno, 
                request.Sexo, request.Nacionalidad, request.Direccion, request.Correo, request.Activo
            });
            return records > 0;
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int id)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();
            var empresaId = await connection.ExecuteScalarAsync<int>("SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1", new { rucEmpresa });
            var query = "DELETE FROM anexos WHERE id = @id AND empresa_id = @empresaId";
            var records = await connection.ExecuteAsync(query, new { id, empresaId });
            return records > 0;
        }
    }
}