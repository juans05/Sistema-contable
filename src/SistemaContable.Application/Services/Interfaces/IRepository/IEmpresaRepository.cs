using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IEmpresaRepository
    {
        Task<PagedResultDto<EEmpresa>> ListarAsync(EmpresaQueryRequest empresaQueryRequest);

        Task<EEmpresa?> ObtenerPorIdAsync(Guid empresaId);

        Task<ApiResponseDto<Guid>> CrearAsync(CreateEmpresaRequest dto);

        Task<ApiResponseDto<bool>> ActualizarAsync(UpdateEmpresaRequest dto);

        Task<ApiResponseDto<bool>> EliminarAsync(Guid empresaId);

        Task<ApiResponseDto<bool>> AsignarContadorAsync(
           AsignarContadorRequest dto, int asignadoPor
        );

        Task<List<EContadorDto>> ListarContadoresAsync();

        Task<ApiResponseDto<bool>> CambiarEstadoAsync(Guid empresaId, bool activo);
    }
}
