using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IProductoService
    {
        Task<(List<ProductoDto> productos, int total)> ListarProductosAsync(string rucEmpresa, int page, int pageSize, string? filtro);
        Task<ProductoDetalleDto?> ObtenerProductoPorIdAsync(int idProducto);
        Task<int> CrearProductoAsync(CrearProductoRequest request, string usuario, string rucEmpresa);
        Task<bool> ActualizarProductoAsync(int idProducto, ActualizarProductoRequest request, string usuario, string rucEmpresa);
        Task<bool> EliminarProductoAsync(int idProducto);
        Task<List<CategoriaDto>> ListarCategoriasAsync();
        Task<List<MarcaDto>> ListarMarcasAsync();
        Task<List<UnidadMedidaDto>> ListarUnidadesMedidaAsync();
        Task<List<MonedaDto>> ListarMonedasAsync();
        Task<List<PlanContableDto>> ListarCuentasContablesAsync();
    }
}
