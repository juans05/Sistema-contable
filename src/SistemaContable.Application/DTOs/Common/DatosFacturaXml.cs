using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Common
{
    public class DatosFacturaXml
    {
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
        public decimal TipoCambio { get; set; }
        public string ClienteRuc { get; set; }
        public string ClienteTipoDocumento { get; set; }
        public string ClienteRazonSocial { get; set; }
        public string TipoOperacion { get; set; }
        public List<DetalleFacturaXml> Detalles { get; set; }
    }
    public class DetalleFacturaXml
    {
        public int NumeroLinea { get; set; }
        public string CodigoProducto { get; set; }
        public string Descripcion { get; set; }
        public string UnidadMedida { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioUnitarioConIgv { get; set; }
        public decimal ValorVenta { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal TotalLinea { get; set; }
        public string TipoAfectacionIgv { get; set; }
    }
}
