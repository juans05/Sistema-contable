using System.Collections.Generic;
using System.Threading.Tasks;
using SistemaContable.Domain.Entities;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface ICategoriaRepository
    {
        Task<List<ECategoria>> ListarAsync(string rucEmpresa, bool? activo = null);
        Task<ECategoria?> ObtenerPorIdAsync(string rucEmpresa, int idCategoria);
        Task<int> CrearAsync(string rucEmpresa, CrearCategoriaRequest request);
        Task<bool> ActualizarAsync(string rucEmpresa, int idCategoria, ActualizarCategoriaRequest request);
        Task<bool> EliminarAsync(string rucEmpresa, int idCategoria);
    }
}