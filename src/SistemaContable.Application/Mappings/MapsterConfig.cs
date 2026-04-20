using Mapster;
using SistemaContable.Domain.Entities;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Mappings
{
    public class MapsterConfig : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<ETipoComprobante, TipoComprobanteDto>();
            config.NewConfig<EAnexo, AnexoDto>();
            config.NewConfig<EPlanContableGeneral, PlanContableDetalleDto>();
            config.NewConfig<ECategoria, CategoriaDto>();
            config.NewConfig<EMarca, MarcaDto>();
        }
    }
}