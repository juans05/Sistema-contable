using Microsoft.Extensions.Logging;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Implementations
{
    public class ProductoService : IProductoService
    {
        private readonly IProductoRepository _productoRepository;
        private readonly ILogger<ProductoService> _logger;

        public ProductoService(
            IProductoRepository productoRepository,
            ILogger<ProductoService> logger)
        {
            _productoRepository = productoRepository;
            _logger = logger;
        }

        public async Task<(List<ProductoDto> productos, int total)> ListarProductosAsync(
            string rucEmpresa, int page, int pageSize, string? filtro)
        {
            try
            {
                return await _productoRepository.ListarAsync(rucEmpresa, page, pageSize, filtro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarProductosAsync");
                throw;
            }
        }

        public async Task<ProductoDetalleDto?> ObtenerProductoPorIdAsync(int idProducto)
        {
            try
            {
                return await _productoRepository.ObtenerPorIdAsync(idProducto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ObtenerProductoPorIdAsync para ID {IdProducto}", idProducto);
                throw;
            }
        }

        public async Task<int> CrearProductoAsync(CrearProductoRequest request, string usuario, string rucEmpresa)
        {
            try
            {
                // ========== VALIDACIONES DE CAMPOS REQUERIDOS ==========
                if (string.IsNullOrWhiteSpace(request.Codigo))
                {
                    throw new ArgumentException("El código (SKU) es requerido");
                }

                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    throw new ArgumentException("El nombre del producto es requerido");
                }

                // ========== VALIDACIONES DE LONGITUD ==========
                if (request.Codigo.Length > 50)
                {
                    throw new ArgumentException("El código no puede exceder 50 caracteres");
                }

                if (request.Nombre.Length > 200)
                {
                    throw new ArgumentException("El nombre no puede exceder 200 caracteres");
                }

                if (!string.IsNullOrEmpty(request.CodigoBarras) && request.CodigoBarras.Length > 50)
                {
                    throw new ArgumentException("El código de barras no puede exceder 50 caracteres");
                }

                if (!string.IsNullOrEmpty(request.CodigoSunat) && request.CodigoSunat.Length > 20)
                {
                    throw new ArgumentException("El código SUNAT no puede exceder 20 caracteres");
                }

                // ========== VALIDACIONES NUMÉRICAS (NO NEGATIVOS) ==========
                if (request.PrecioCompra < 0)
                {
                    throw new ArgumentException("El costo de compra no puede ser negativo");
                }

                if (request.MargenUtilidad.HasValue && request.MargenUtilidad.Value < 0)
                {
                    throw new ArgumentException("El margen de utilidad no puede ser negativo");
                }

                if (request.MargenUtilidad.HasValue && request.MargenUtilidad.Value > 1000)
                {
                    throw new ArgumentException("El margen de utilidad no puede exceder 1000%");
                }

                if (request.StockMinimo < 0)
                {
                    throw new ArgumentException("El stock mínimo no puede ser negativo");
                }

                if (request.StockMaximo < 0)
                {
                    throw new ArgumentException("El stock máximo no puede ser negativo");
                }

                // ========== VALIDACIONES DE LÓGICA DE NEGOCIO ==========
                if (request.StockMaximo > 0 && request.StockMinimo > request.StockMaximo)
                {
                    throw new ArgumentException("El stock mínimo no puede ser mayor que el stock máximo");
                }

                // Validar SKU único por empresa
                var existeSku = await _productoRepository.ExisteSkuAsync(rucEmpresa, request.Codigo);
                if (existeSku)
                {
                    throw new InvalidOperationException($"Ya existe un producto con el código '{request.Codigo}' en esta empresa");
                }

                // Validación de precio de venta sugerido
                var precioVentaSugerido = request.MargenUtilidad.HasValue
                    ? request.PrecioCompra * (1 + request.MargenUtilidad.Value / 100)
                    : request.PrecioCompra;

                if (precioVentaSugerido <= request.PrecioCompra && request.MargenUtilidad.HasValue && request.MargenUtilidad.Value > 0)
                {
                    _logger.LogWarning("Precio de venta sugerido es menor o igual al costo (posible error de cálculo)");
                }

                return await _productoRepository.CrearAsync(rucEmpresa, request, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en CrearProductoAsync");
                throw;
            }
        }

        public async Task<bool> ActualizarProductoAsync(
            int idProducto, ActualizarProductoRequest request, string usuario, string rucEmpresa)
        {
            try
            {
                // Verificar que el producto existe
                var productoExistente = await _productoRepository.ObtenerPorIdAsync(idProducto);
                if (productoExistente == null)
                {
                    throw new InvalidOperationException($"No se encontró el producto con ID {idProducto}");
                }

                // ========== VALIDACIONES DE CAMPOS REQUERIDOS ==========
                if (string.IsNullOrWhiteSpace(request.Codigo))
                {
                    throw new ArgumentException("El código (SKU) es requerido");
                }

                if (string.IsNullOrWhiteSpace(request.Nombre))
                {
                    throw new ArgumentException("El nombre del producto es requerido");
                }

                // ========== VALIDACIONES DE LONGITUD ==========
                if (request.Codigo.Length > 50)
                {
                    throw new ArgumentException("El código no puede exceder 50 caracteres");
                }

                if (request.Nombre.Length > 200)
                {
                    throw new ArgumentException("El nombre no puede exceder 200 caracteres");
                }

                if (!string.IsNullOrEmpty(request.CodigoBarras) && request.CodigoBarras.Length > 50)
                {
                    throw new ArgumentException("El código de barras no puede exceder 50 caracteres");
                }

                if (!string.IsNullOrEmpty(request.CodigoSunat) && request.CodigoSunat.Length > 20)
                {
                    throw new ArgumentException("El código SUNAT no puede exceder 20 caracteres");
                }

                // ========== VALIDACIONES NUMÉRICAS (NO NEGATIVOS) ==========
                if (request.PrecioCompra < 0)
                {
                    throw new ArgumentException("El costo de compra no puede ser negativo");
                }

                if (request.MargenUtilidad.HasValue && request.MargenUtilidad.Value < 0)
                {
                    throw new ArgumentException("El margen de utilidad no puede ser negativo");
                }

                if (request.MargenUtilidad.HasValue && request.MargenUtilidad.Value > 1000)
                {
                    throw new ArgumentException("El margen de utilidad no puede exceder 1000%");
                }

                if (request.StockMinimo < 0)
                {
                    throw new ArgumentException("El stock mínimo no puede ser negativo");
                }

                if (request.StockMaximo < 0)
                {
                    throw new ArgumentException("El stock máximo no puede ser negativo");
                }

                // ========== VALIDACIONES DE LÓGICA DE NEGOCIO ==========
                if (request.StockMaximo > 0 && request.StockMinimo > request.StockMaximo)
                {
                    throw new ArgumentException("El stock mínimo no puede ser mayor que el stock máximo");
                }

                // Validar SKU único (excluyendo el producto actual)
                var existeSku = await _productoRepository.ExisteSkuAsync(rucEmpresa, request.Codigo, idProducto);
                if (existeSku)
                {
                    throw new InvalidOperationException($"Ya existe otro producto con el código '{request.Codigo}' en esta empresa");
                }

                return await _productoRepository.ActualizarAsync(idProducto, request, usuario);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ActualizarProductoAsync para ID {IdProducto}", idProducto);
                throw;
            }
        }

        public async Task<bool> EliminarProductoAsync(int idProducto)
        {
            try
            {
                // Verificar que el producto existe
                var producto = await _productoRepository.ObtenerPorIdAsync(idProducto);
                if (producto == null)
                {
                    throw new InvalidOperationException($"No se encontró el producto con ID {idProducto}");
                }

                return await _productoRepository.EliminarAsync(idProducto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en EliminarProductoAsync para ID {IdProducto}", idProducto);
                throw;
            }
        }

        public async Task<List<CategoriaDto>> ListarCategoriasAsync()
        {
            try
            {
                return await _productoRepository.ListarCategoriasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarCategoriasAsync");
                throw;
            }
        }

        public async Task<List<MarcaDto>> ListarMarcasAsync()
        {
            try
            {
                return await _productoRepository.ListarMarcasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarMarcasAsync");
                throw;
            }
        }

        public async Task<List<UnidadMedidaDto>> ListarUnidadesMedidaAsync()
        {
            try
            {
                return await _productoRepository.ListarUnidadesMedidaAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarUnidadesMedidaAsync");
                throw;
            }
        }

        public async Task<List<MonedaDto>> ListarMonedasAsync()
        {
            try
            {
                return await _productoRepository.ListarMonedasAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarMonedasAsync");
                throw;
            }
        }

        public async Task<List<PlanContableDto>> ListarCuentasContablesAsync()
        {
            try
            {
                return await _productoRepository.ListarCuentasContablesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en ListarCuentasContablesAsync");
                throw;
            }
        }
    }
}
