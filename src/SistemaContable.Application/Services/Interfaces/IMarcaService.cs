using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IMarcaService
    {
        Task<List<MarcaDto>> ListarAsync(string rucEmpresa, bool? activo = null);
        Task<MarcaDto?> ObtenerPorIdAsync(string rucEmpresa, int idMarca);
        Task<int> CrearAsync(string rucEmpresa, CrearMarcaRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int idMarca, ActualizarMarcaRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int idMarca);
    }
}