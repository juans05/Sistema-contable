using System;

namespace SistemaContable.Domain.Entities
{
    public class EProducto
    {
        // Mapeo a tabla productos existente
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        
        // Identificación
        public string Codigo { get; set; } // SKU
        public string? CodigoBarras { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        
        // Categorización (IDs de relaciones)
        public int? CategoriaId { get; set; }
        public int? MarcaId { get; set; }
        public int? LineaId { get; set; }
        
        // Fiscal y Contable
        public bool AfectoIgv { get; set; } // Tipo de Impuesto
        public int? CuentaVentaId { get; set; } // Cuenta Contable Ventas
        public int? CuentaCostoId { get; set; } // Cuenta Contable Costos
        public string? CodigoSunat { get; set; } // Código Fiscal Externo
        public bool RequiereInspeccion { get; set; } // Indicador de Retención
        
        // Precios y Costos
        public decimal PrecioCompra { get; set; } // Costo de Compra
        public decimal? MargenUtilidad { get; set; } // Margen de Utilidad %
        public decimal PrecioVenta { get; set; } // Precio de Venta Sugerido
        public int? MonedaId { get; set; }
        public Moneda? Moneda { get; set; } // Navegación

        // Inventario
        public int? UnidadMedidaId { get; set; }
        public decimal StockMinimo { get; set; }
        public decimal StockMaximo { get; set; }
        public decimal StockActual { get; set; }
        public string? UbicacionFisica { get; set; }
        
        // Cuentas Contables Adicionales
        public int? CuentaInventarioId { get; set; }
        public int? CuentaCompraId { get; set; }
        
        // Impuestos y Retenciones
        public bool SujetoRetencion { get; set; } // Nuevo campo solicitado

        public bool MargenInventario { get; set; } // ¿Es Inventariable?
        public bool MargenLotes { get; set; } // Manejo de Lotes
        public bool MargenSeries { get; set; } // Manejo de Series
        public bool MargenVencimiento { get; set; } // Tiene Vencimiento
        
        // Metadatos
        public string? ImagenUrl { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
