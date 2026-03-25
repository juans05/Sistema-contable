using SistemaContable.Domain.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IProductoRepository
    {
        Task<(List<ProductoDto> productos, int total)> ListarAsync(string rucEmpresa, int page, int pageSize, string? filtro = null);
        Task<ProductoDetalleDto?> ObtenerPorIdAsync(int idProducto);
        Task<int> CrearAsync(string rucEmpresa, CrearProductoRequest request, string usuario);
        Task<bool> ActualizarAsync(int idProducto, ActualizarProductoRequest request, string usuario);
        Task<bool> EliminarAsync(int idProducto);
        
        // Catálogos
        Task<List<CategoriaDto>> ListarCategoriasAsync();
        Task<List<MarcaDto>> ListarMarcasAsync();
        Task<List<UnidadMedidaDto>> ListarUnidadesMedidaAsync();
        Task<List<MonedaDto>> ListarMonedasAsync();
        Task<List<PlanContableDto>> ListarCuentasContablesAsync();
        
        // Validaciones
        Task<bool> ExisteSkuAsync(string rucEmpresa, string codigoSku, int? idProductoExcluir = null);
    }
}
