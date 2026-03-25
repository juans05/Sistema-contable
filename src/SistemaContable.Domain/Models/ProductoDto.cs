using System;

namespace SistemaContable.Domain.Models
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public string? Categoria { get; set; }
        public string? Marca { get; set; }
        public decimal PrecioVenta { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal? MargenUtilidad { get; set; }
        public string? UnidadMedida { get; set; }
        public decimal StockActual { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public bool AfectoIgv { get; set; }
        public bool Activo { get; set; }
    }

    public class ProductoDetalleDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        
        // Identificación
        public string Codigo { get; set; }
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        
        // Categorización
        public int? CategoriaId { get; set; }
        public string? Categoria { get; set; }
        public int? MarcaId { get; set; }
        public string? Marca { get; set; }
        public int? LineaId { get; set; }
        public string? Linea { get; set; }
        
        // Fiscal y Contable
        public bool AfectoIgv { get; set; }
        public int? CuentaVentaId { get; set; }
        public string? CuentaVentaCodigo { get; set; }
        public int? CuentaCostoId { get; set; }
        public string? CuentaCostoCodigo { get; set; }
        public string? CodigoSunat { get; set; }
        public bool RequiereInspeccion { get; set; }
        
        // Precios y Costos
        public decimal PrecioCompra { get; set; }
        public decimal? MargenUtilidad { get; set; }
        public decimal PrecioVenta { get; set; }
        public int? MonedaId { get; set; }
        public string? Moneda { get; set; }
        
        // Inventario
        public int? UnidadMedidaId { get; set; }
        public string? UnidadMedida { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public decimal StockActual { get; set; }
        public string? UbicacionFisica { get; set; }
        public bool MargenInventario { get; set; }
        public bool MargenLotes { get; set; }
        public bool MargenSeries { get; set; }
        public bool MargenVencimiento { get; set; }
        
        // Metadatos
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public bool SujetoRetencion { get; set; }
        public int? CuentaCompraId { get; set; }
        public int? CuentaInventarioId { get; set; }
    }

    public class CrearProductoRequest
    {
        public string Codigo { get; set; }
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public int? CategoriaId { get; set; }
        public int? MarcaId { get; set; }
        public int? LineaId { get; set; }
        public bool AfectoIgv { get; set; } = true;
        public int? CuentaVentaId { get; set; }
        public int? CuentaCostoId { get; set; }
        public string? CodigoSunat { get; set; }
        public bool RequiereInspeccion { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal? MargenUtilidad { get; set; }
        public int? MonedaId { get; set; }
        public int? UnidadMedidaId { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public string? UbicacionFisica { get; set; }
        public bool MargenInventario { get; set; } = true;
        public bool MargenLotes { get; set; }
        public bool MargenSeries { get; set; }
        public bool MargenVencimiento { get; set; }
        public string? ImagenUrl { get; set; }
        public bool SujetoRetencion { get; set; }
        public int? CuentaCompraId { get; set; }
        public int? CuentaInventarioId { get; set; }
    }

    public class ActualizarProductoRequest : CrearProductoRequest
    {
        public bool Activo { get; set; } = true;
    }

    public class CategoriaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
    }

    public class MarcaDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
    }

    public class UnidadMedidaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
    }

    public class ListarProductosResponse
    {
        public List<ProductoDto> Productos { get; set; }
        public int Total { get; set; }
    }

    public class MonedaDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Simbolo { get; set; }
    }

    public class PlanContableDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string TipoCuenta { get; set; }
    }
}
