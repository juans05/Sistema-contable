using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class FacturaElectronicaDto
    {
        public int IdFacturaElectronica { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string NumeroCompleto { get; set; }
        public string TipoDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string Moneda { get; set; }
        public decimal MontoBase { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal MontoTotal { get; set; }
        public string XmlOriginal { get; set; }
        public string CodigoHash { get; set; }
    }

    public class RegistroVentaDto
    {
        public int IdRegVenta { get; set; }
        public int? IdFacturaElectronica { get; set; }
        public string RucCliente { get; set; }
        public string Periodo { get; set; }
        public string RsCliente { get; set; }
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
        public string TipOperaSunat { get; set; }
    }

    public class RegistroVentaDetalleDto
    {
        public int NumeroLinea { get; set; }
        public string CodigoProducto { get; set; }
        public string DescripcionProducto { get; set; }
        public string UnidadMedida { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioUnitarioConIgv { get; set; }
        public decimal ValorVenta { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal TotalLinea { get; set; }
        public string TipoAfectacionIgv { get; set; }
        public decimal PorcentajeIgv { get; set; }
    }
}
