using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public  interface IEmpresaService
    {
        Task<PagedResultDto<EEmpresa>> ListarEmpresasAsync(EmpresaQueryRequest empresaQueryRequest);

        Task<EEmpresa?> ObtenerEmpresaPorIdAsync(Guid empresaId);

        Task<ApiResponseDto<Guid>> CrearEmpresaAsync(CreateEmpresaRequest dto);

        Task<ApiResponseDto<bool>> ActualizarEmpresaAsync(UpdateEmpresaRequest dto);

        Task<ApiResponseDto<bool>> EliminarEmpresaAsync(Guid empresaId);

        Task<ApiResponseDto<bool>> AsignarContadorAsync(AsignarContadorRequest dto, int asignadoPor);

        Task<List<EContadorDto>> ListarContadoresAsync();
        Task<EContadorDto> ObtenerContadorPorIdAsync(int id);
        Task<CambiarEstadoResponse> CambiarEstadoEmpresaAsync(CambiarEstadoRequest request);
    }
}

