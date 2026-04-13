using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using SistemaContable.Infrastructure.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class EmpresaRepository : IEmpresaRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<EmpresaRepository> _logger;

        public EmpresaRepository(
            NpgsqlDataSource dataSource,
            ILogger<EmpresaRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ApiResponseDto<bool>> ActualizarAsync(UpdateEmpresaRequest dto)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_id = dto.id,
                    p_razon_social = dto.razonSocial,
                    p_nombre_comercial = dto.nombreComercial,
                    p_ruc = dto.ruc,
                    p_direccion = dto.direccion,
                    p_telefono = dto.telefono,
                    p_email = dto.email,
                    p_web = dto.web,
                    p_regimen_tributario = dto.regimenTributario,
                    p_tipo_contribuyente = dto.tipoContribuyente,
                    p_fecha_constitucion = dto.fechaConstitucion,
                    p_representante_legal = dto.representanteLegal,
                    p_dni_representante = dto.dniRepresentante,
                    p_logo_url = dto.logoUrl,
                    p_config = (object?)null
                };

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_actualizar(
                    @p_id, @p_razon_social, @p_nombre_comercial, @p_ruc,
                    @p_direccion, @p_telefono, @p_email, @p_web,
                    @p_regimen_tributario, @p_tipo_contribuyente, @p_fecha_constitucion,
                    @p_representante_legal, @p_dni_representante, @p_logo_url, @p_config
                )",
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: 30
                );

                return new ApiResponseDto<bool>
                {
                    Success = result.success,
                    Message = result.message,
                    Data = result.success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empresa {EmpresaId}", dto.id);
                throw;
            }
        }

        public async Task<ApiResponseDto<bool>> AsignarContadorAsync(AsignarContadorRequest dto, int asignadoPor)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_empresa_id = dto.EmpresaId,
                    p_contador_id = dto.ContadorId,
                    p_asignado_por = asignadoPor,
                    p_puede_crear_usuarios = dto.PuedeCrearUsuarios,
                    p_puede_modificar_config = dto.PuedeModificarConfig,
                    p_puede_eliminar = dto.PuedeEliminar
                };

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_asignar_contador(
                    @p_empresa_id, @p_contador_id, @p_asignado_por,
                    @p_puede_crear_usuarios, @p_puede_modificar_config, @p_puede_eliminar
                )",
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: 10
                );

                return new ApiResponseDto<bool>
                {
                    Success = result.success,
                    Message = result.message,
                    Data = result.success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar contador");
                throw;
            }
        }

        public async Task<ApiResponseDto<bool>> CambiarEstadoAsync(Guid empresaId, bool activo)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_id = empresaId,
                    p_activo = activo
                };

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_cambiar_estado(@p_id, @p_activo)",
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: 10
                );

                if (result == null)
                {
                    _logger.LogWarning(
                        "No se obtuvo respuesta al cambiar estado de empresa {EmpresaId} a {Estado}",
                        empresaId,
                        activo
                    );

                    return new ApiResponseDto<bool>
                    {
                        Success = false,
                        Message = "Error al cambiar estado de la empresa",
                        Data = false
                    };
                }

                return new ApiResponseDto<bool>
                {
                    Success = result.success,
                    Message = result.message,
                    Data = result.success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cambiar estado de empresa {EmpresaId} a {Estado}",
                    empresaId,
                    activo
                );
                throw;
            }
        }

        public async Task<ApiResponseDto<Guid>> CrearAsync(CreateEmpresaRequest empresa)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();



              
                var result = await connection.QueryFirstOrDefaultAsync<SpEmpresaCrearResult>(
                                 @"SELECT * FROM ""suizaConta"".sp_empresa_crear(
                                    @p_razon_social, @p_nombre_comercial, @p_ruc, @p_direccion,
                                    @p_telefono, @p_email, @p_web, @p_regimen_tributario,
                                    @p_tipo_contribuyente, @p_fecha_constitucion,
                                    @p_representante_legal, @p_dni_representante, @p_logo_url, @p_contador_id
                                )",
                                 new
                                 {
                                     p_razon_social = empresa.razonSocial,
                                     p_nombre_comercial = empresa.nombreComercial ?? string.Empty,
                                     p_ruc = empresa.ruc,
                                     p_direccion = empresa.direccion ?? string.Empty,
                                     p_telefono = empresa.telefono ?? string.Empty,
                                     p_email = empresa.email ?? string.Empty,
                                     p_web = empresa.web ?? string.Empty,
                                     p_regimen_tributario = empresa.regimenTributario ?? string.Empty,
                                     p_tipo_contribuyente = empresa.tipoContribuyente ?? string.Empty,
                                     p_fecha_constitucion = empresa.fechaConstitucion ?? (object)DBNull.Value, // ✅ CRÍTICO: .Date
                                     p_representante_legal = empresa.representanteLegal ?? string.Empty,
                                     p_dni_representante = empresa.dniRepresentante ?? string.Empty,
                                     p_logo_url = empresa.logoUrl ?? (object)DBNull.Value,
                                     p_contador_id = empresa.contadorId ?? (object)DBNull.Value
                                 },
                                 commandTimeout: 30
                             );

                if (result == null)
                {
                    throw new Exception("El stored procedure no devolvió resultado");
                }

                if (!result.Success)
                {
                    _logger.LogWarning("Error al crear empresa: {Message}", result.Message);
                    return new ApiResponseDto<Guid>
                    {
                        Success = false,
                        Message = result.Message,
                        Data = Guid.Empty
                    };
                }

                _logger.LogInformation("Empresa creada exitosamente: {EmpresaId}", result.Id);

                return new ApiResponseDto<Guid>
                {
                    Success = true,
                    Message = result.Message,
                    Data = result.Id.GetValueOrDefault()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empresa");
                throw;
            }
        }

        public async Task<ApiResponseDto<bool>> EliminarAsync(Guid empresaId)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_eliminar(@p_id)",
                    new { p_id = empresaId },
                    commandType: CommandType.Text,
                    commandTimeout: 10
                );

                return new ApiResponseDto<bool>
                {
                    Success = result.success,
                    Message = result.message,
                    Data = result.success
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<PagedResultDto<EEmpresa>> ListarAsync(EmpresaQueryRequest empresaQueryRequest)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_usuario_id = empresaQueryRequest.UsuarioId,
                    p_es_contador = false,
                    p_activo = empresaQueryRequest.Activo,
                    p_search = empresaQueryRequest.Search,
                    p_page_number = empresaQueryRequest.PageNumber,
                    p_page_size = empresaQueryRequest.PageSize
                };

                var result = await connection.QueryAsync<EEmpresa>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_listar(
                    @p_usuario_id, @p_es_contador, @p_activo, 
                    @p_search, @p_page_number, @p_page_size
                )",
                    parameters,
                    commandType: CommandType.Text,
                    commandTimeout: 30
                );

                var empresas = result.ToList();
                var totalRecords = empresas.Count();

                return new PagedResultDto<EEmpresa>
                {
                    TotalRecords = totalRecords,
                    PageNumber = empresaQueryRequest.PageNumber,
                    PageSize = empresaQueryRequest.PageSize,
                    Data = empresas
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar empresas");
                throw;
            }
        }

        public async Task<List<EContadorDto>> ListarContadoresAsync()
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var contadores = await connection.QueryAsync<EContadorDto>(
                    @"SELECT * FROM ""suizaConta"".sp_contadores_listar()",
                    commandType: CommandType.Text,
                    commandTimeout: 10
                );

                return contadores.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar contadores");
                throw;
            }
        }

        public async Task<EEmpresa?> ObtenerPorIdAsync(Guid empresaId)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var empresa = await connection.QueryFirstOrDefaultAsync<EEmpresa>(
                    @"SELECT * FROM ""suizaConta"".sp_empresa_obtener_por_id(@p_empresa_id)",
                    new { p_empresa_id = empresaId },
                    commandType: CommandType.Text,
                    commandTimeout: 10
                );

                return empresa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresa {EmpresaId}", empresaId);
                throw;
            }
        }
    }
}
