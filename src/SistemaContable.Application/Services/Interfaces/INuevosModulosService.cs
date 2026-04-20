using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ITipoComprobanteService
    {
        Task<List<TipoComprobanteDto>> ListarAsync(bool? activo = null);
        Task<TipoComprobanteDto?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(CrearTipoComprobanteRequest request);
        Task<bool> ActualizarAsync(int id, ActualizarTipoComprobanteRequest request);
        Task<bool> EliminarAsync(int id);
    }
    
    public interface IAnexoService
    {
        Task<List<AnexoDto>> ListarAsync(string rucEmpresa, string? tipoAnexo = null, bool? activo = null);
        Task<AnexoDto?> ObtenerPorIdAsync(string rucEmpresa, int id);
        Task<int> CrearAsync(string rucEmpresa, CrearAnexoRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarAnexoRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int id);
    }
    
    public interface IPlanContableService
    {
        Task<List<PlanContableDetalleDto>> ListarAsync(string rucEmpresa, bool? activo = null);
        Task<PlanContableDetalleDto?> ObtenerPorIdAsync(string rucEmpresa, int id);
        Task<int> CrearAsync(string rucEmpresa, CrearPlanContableRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarPlanContableRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int id);
    }
}