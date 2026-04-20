using System.Collections.Generic;
using System.Threading.Tasks;
using Mapster;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Implementations
{
    public class TipoComprobanteService : ITipoComprobanteService
    {
        private readonly ITipoComprobanteRepository _repository;
        public TipoComprobanteService(ITipoComprobanteRepository repository) => _repository = repository;

        public async Task<List<TipoComprobanteDto>> ListarAsync(bool? activo = null)
        {
            var entities = await _repository.ListarAsync(activo);
            return entities.Adapt<List<TipoComprobanteDto>>();
        }

        public async Task<TipoComprobanteDto?> ObtenerPorIdAsync(int id)
        {
            var entity = await _repository.ObtenerPorIdAsync(id);
            return entity?.Adapt<TipoComprobanteDto>();
        }

        public Task<int> CrearAsync(CrearTipoComprobanteRequest request) => _repository.CrearAsync(request);
        public Task<bool> ActualizarAsync(int id, ActualizarTipoComprobanteRequest request) => _repository.ActualizarAsync(id, request);
        public Task<bool> EliminarAsync(int id) => _repository.EliminarAsync(id);
    }

    public class AnexoService : IAnexoService
    {
        private readonly IAnexoRepository _repository;
        public AnexoService(IAnexoRepository repository) => _repository = repository;

        public async Task<List<AnexoDto>> ListarAsync(string rucEmpresa, string? tipoAnexo = null, bool? activo = null)
        {
            var entities = await _repository.ListarAsync(rucEmpresa, tipoAnexo, activo);
            return entities.Adapt<List<AnexoDto>>();
        }

        public async Task<AnexoDto?> ObtenerPorIdAsync(string rucEmpresa, int id)
        {
            var entity = await _repository.ObtenerPorIdAsync(rucEmpresa, id);
            return entity?.Adapt<AnexoDto>();
        }

        public Task<int> CrearAsync(string rucEmpresa, CrearAnexoRequest request) => _repository.CrearAsync(rucEmpresa, request);
        public Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarAnexoRequest request) => _repository.ActualizarAsync(rucEmpresa, id, request);
        public Task<bool> EliminarAsync(string rucEmpresa, int id) => _repository.EliminarAsync(rucEmpresa, id);
    }

    public class PlanContableService : IPlanContableService
    {
        private readonly IPlanContableRepository _repository;
        public PlanContableService(IPlanContableRepository repository) => _repository = repository;

        public async Task<List<PlanContableDetalleDto>> ListarAsync(string rucEmpresa, bool? activo = null)
        {
            var entities = await _repository.ListarAsync(rucEmpresa, activo);
            return entities.Adapt<List<PlanContableDetalleDto>>();
        }

        public async Task<PlanContableDetalleDto?> ObtenerPorIdAsync(string rucEmpresa, int id)
        {
            var entity = await _repository.ObtenerPorIdAsync(rucEmpresa, id);
            return entity?.Adapt<PlanContableDetalleDto>();
        }

        public Task<int> CrearAsync(string rucEmpresa, CrearPlanContableRequest request) => _repository.CrearAsync(rucEmpresa, request);
        public Task<bool> ActualizarAsync(string rucEmpresa, int id, ActualizarPlanContableRequest request) => _repository.ActualizarAsync(rucEmpresa, id, request);
        public Task<bool> EliminarAsync(string rucEmpresa, int id) => _repository.EliminarAsync(rucEmpresa, id);
    }
}