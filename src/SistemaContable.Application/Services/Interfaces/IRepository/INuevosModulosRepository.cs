using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface ITipoComprobanteRepository
    {
        Task<List<ETipoComprobante>> ListarAsync(bool? activo = null);
        Task<ETipoComprobante?> ObtenerPorIdAsync(int id);
        Task<int> CrearAsync(CrearTipoComprobanteRequest request);
        Task<bool> ActualizarAsync(int id, ActualizarTipoComprobanteRequest request);
        Task<bool> EliminarAsync(int id);
    }

    public interface IAnexoRepository
    {
        Task<List<EAnexo>> ListarAsync(string rucEmpresa, string? tipoAnexo = null, bool? activo = null);
        Task<EAnexo?> ObtenerPorIdAsync(string rucEmpresa, int id);
        Task<int> CrearAsync(string rucEmpresa, CrearAnexoRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarAnexoRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int id);
    }

    public interface IPlanContableRepository
    {
        Task<List<EPlanContableGeneral>> ListarAsync(string rucEmpresa, bool? activo = null);
        Task<EPlanContableGeneral?> ObtenerPorIdAsync(string rucEmpresa, int id);
        Task<int> CrearAsync(string rucEmpresa, CrearPlanContableRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarPlanContableRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int id);
    }
}