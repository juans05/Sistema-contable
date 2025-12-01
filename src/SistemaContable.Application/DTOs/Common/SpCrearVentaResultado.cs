using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Common
{
    public class SpCrearVentaResultado
    {
        public int? OIdRegVenta { get; set; }
        public string OMensaje { get; set; }
    }

    public class VentaPorIdResultado
    {
        public int IdRegVenta { get; set; }
        public int? IdFacturaElectronica { get; set; }
        public string RucCliente { get; set; }
        public string Periodo { get; set; }
        public string RSCliente { get; set; }
        public string TipoDoc { get; set; }
        public string SerieDoc { get; set; }
        public string NumDoc { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public decimal TipCambio { get; set; }
        public string TipoDocCliente { get; set; }
        public string Moneda { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ImpIgv { get; set; }
        public decimal TotalDoc { get; set; }
        public string EstadoDoc { get; set; }
        public string TipOperaSunat { get; set; }
        public DateTime CreatedAt { get; set; }
        // Detalles
        public int? DetalleId { get; set; }
        public int? DetalleNumeroLinea { get; set; }
        public string DetalleCodigoProducto { get; set; }
        public string DetalleDescripcion { get; set; }
        public string DetalleUnidadMedida { get; set; }
        public decimal? DetalleCantidad { get; set; }
        public decimal? DetallePrecioUnitario { get; set; }
        public decimal? DetallePrecioUnitarioConIgv { get; set; }
        public decimal? DetalleValorVenta { get; set; }
        public decimal? DetalleDescuento { get; set; }
        public decimal? DetalleMontoIgv { get; set; }
        public decimal? DetalleTotalLinea { get; set; }
        public string DetalleTipoAfectacionIgv { get; set; }
        public decimal? DetallePorcentajeIgv { get; set; }
    }
}
