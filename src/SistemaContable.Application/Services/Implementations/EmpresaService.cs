using Microsoft.Extensions.Logging;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Implementations
{
    public class EmpresaService : IEmpresaService
    {
        private readonly IEmpresaRepository _empresaRepository;
        private readonly ILogger<EmpresaService> _logger;

        public EmpresaService(
            IEmpresaRepository empresaRepository,
            ILogger<EmpresaService> logger)
        {
            _empresaRepository = empresaRepository;
            _logger = logger;
        }
        public async Task<ApiResponseDto<bool>> ActualizarEmpresaAsync(UpdateEmpresaRequest dto)
        {
            try
            {
                return await _empresaRepository.ActualizarAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarEmpresaAsync para {EmpresaId}", dto.Id);
                throw;
            }
        }

        public async Task<ApiResponseDto<bool>> AsignarContadorAsync(AsignarContadorRequest dto, int asignadoPor)
        {
            try
            {
                return await _empresaRepository.AsignarContadorAsync(dto, asignadoPor);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AsignarContadorAsync");
                throw;
            }
        }

        public async Task<CambiarEstadoResponse> CambiarEstadoEmpresaAsync(CambiarEstadoRequest request)
        {
            try
            {
                // Validar que la empresa existe antes de cambiar estado
                var empresaExistente = await _empresaRepository.ObtenerPorIdAsync(request.EmpresaId);

                if (empresaExistente == null)
                {
                    return new CambiarEstadoResponse
                    {
                        Success = false,
                        Message = "Empresa no encontrada",
                        EmpresaId = request.EmpresaId,
                        NuevoEstado = request.Activo
                    };
                }

                // Verificar si el estado ya es el mismo
                if (empresaExistente.activo == request.Activo)
                {
                    var estadoActual = request.Activo ? "activada" : "desactivada";
                    return new CambiarEstadoResponse
                    {
                        Success = true,
                        Message = $"La empresa ya se encuentra {estadoActual}",
                        EmpresaId = request.EmpresaId,
                        NuevoEstado = request.Activo
                    };
                }

                // Cambiar el estado
                var resultado = await _empresaRepository.CambiarEstadoAsync(
                    request.EmpresaId,
                    request.Activo
                );

                return new CambiarEstadoResponse
                {
                    Success = resultado.Success,
                    Message = resultado.Message,
                    EmpresaId = request.EmpresaId,
                    NuevoEstado = request.Activo
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error al cambiar estado de empresa {EmpresaId} a {Estado}",
                    request.EmpresaId,
                    request.Activo
                );

                return new CambiarEstadoResponse
                {
                    Success = false,
                    Message = "Error al cambiar estado de la empresa",
                    EmpresaId = request.EmpresaId,
                    NuevoEstado = request.Activo
                };
            }
        }

        public async Task<ApiResponseDto<Guid>> CrearEmpresaAsync(CreateEmpresaRequest dto)
        {
            try
            {
                return await _empresaRepository.CrearAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearEmpresaAsync");
                throw;
            }
        }

        public async Task<ApiResponseDto<bool>> EliminarEmpresaAsync(Guid empresaId)
        {
            try
            {
                return await _empresaRepository.EliminarAsync(empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarEmpresaAsync para {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<EContadorDto>> ListarContadoresAsync()
        {
            try
            {
                return await _empresaRepository.ListarContadoresAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarContadoresAsync");
                throw;
            }
        }

        public async Task<PagedResultDto<EEmpresa>> ListarEmpresasAsync(EmpresaQueryRequest empresaQueryRequest)
        {
            try
            {
                return await _empresaRepository.ListarAsync(empresaQueryRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarEmpresasAsync");
                throw;
            }
        }

        public async Task<EEmpresa?> ObtenerEmpresaPorIdAsync(Guid empresaId)
        {
            try
            {
                return await _empresaRepository.ObtenerPorIdAsync(empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerEmpresaPorIdAsync para {EmpresaId}", empresaId);
                throw;
            }
        }
    }
}
