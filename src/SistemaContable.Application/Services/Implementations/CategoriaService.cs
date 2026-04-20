using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Implementations
{
    public class CategoriaService : ICategoriaService
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriaService(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        public async Task<List<CategoriaDto>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            var entities = await _categoriaRepository.ListarAsync(rucEmpresa, activo);
            return entities.Adapt<List<CategoriaDto>>();
        }

        public async Task<CategoriaDto?> ObtenerPorIdAsync(string rucEmpresa, int idCategoria)
        {
            var entity = await _categoriaRepository.ObtenerPorIdAsync(rucEmpresa, idCategoria);
            return entity?.Adapt<CategoriaDto>();
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearCategoriaRequest request)
        {
            return await _categoriaRepository.CrearAsync(rucEmpresa, request);
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int idCategoria, ActualizarCategoriaRequest request)
        {
            return await _categoriaRepository.ActualizarAsync(rucEmpresa, idCategoria, request);
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int idCategoria)
        {
            return await _categoriaRepository.EliminarAsync(rucEmpresa, idCategoria);
        }
    }
}