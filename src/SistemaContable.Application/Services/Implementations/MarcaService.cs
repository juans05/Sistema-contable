using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Implementations
{
    public class MarcaService : IMarcaService
    {
        private readonly IMarcaRepository _marcaRepository;

        public MarcaService(IMarcaRepository marcaRepository)
        {
            _marcaRepository = marcaRepository;
        }

        public async Task<List<MarcaDto>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            var entities = await _marcaRepository.ListarAsync(rucEmpresa, activo);
            return entities.Adapt<List<MarcaDto>>();
        }

        public async Task<MarcaDto?> ObtenerPorIdAsync(string rucEmpresa, int idMarca)
        {
            var entity = await _marcaRepository.ObtenerPorIdAsync(rucEmpresa, idMarca);
            return entity?.Adapt<MarcaDto>();
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearMarcaRequest request)
        {
            return await _marcaRepository.CrearAsync(rucEmpresa, request);
        }

        public async Task<bool> ActualizarAsync(string rucEmpresa, int idMarca, ActualizarMarcaRequest request)
        {
            return await _marcaRepository.ActualizarAsync(rucEmpresa, idMarca, request);
        }

        public async Task<bool> EliminarAsync(string rucEmpresa, int idMarca)
        {
            return await _marcaRepository.EliminarAsync(rucEmpresa, idMarca);
        }
    }
}