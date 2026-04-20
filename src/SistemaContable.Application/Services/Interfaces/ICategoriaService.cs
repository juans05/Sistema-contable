using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ICategoriaService
    {
        Task<List<CategoriaDto>> ListarAsync(string rucEmpresa, bool? activo = null);
        Task<CategoriaDto?> ObtenerPorIdAsync(string rucEmpresa, int idCategoria);
        Task<int> CrearAsync(string rucEmpresa, CrearCategoriaRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int idCategoria, ActualizarCategoriaRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int idCategoria);
    }
}