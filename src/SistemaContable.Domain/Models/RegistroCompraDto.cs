using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class RegistroCompraDto
    {
        public int IdRegCompras { get; set; }
        public int? IdFacturaCompraElectronica { get; set; }
        public string IdRucProv { get; set; }
        public string Periodo { get; set; }
        public string NombreProv { get; set; }
        public string TipDocumento { get; set; }
        public string SerieDocumento { get; set; }
        public string NoDocumento { get; set; }
        public string FEmisc { get; set; }
        public string? FVcto { get; set; }
        public decimal TipCambio { get; set; }
        public string Moneda { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ImpIgv { get; set; }
        public decimal TotalDoc { get; set; }
        public string TipOperaSunat { get; set; }
        public int estadoDocumento { get; set; }
    }

    public class RegistroCompraDetalleDto 
    {
        public int NumeroLinea { get; set; }
        public string CodigoProducto { get; set; }
        public string DescripcionProducto { get; set; }
        public string UnidadMedida { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal PrecioUnitarioConIgv { get; set; }
        public decimal ValorCompra { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal TotalLinea { get; set; }
        public string TipoAfectacionIgv { get; set; }
        public decimal PorcentajeIgv { get; set; }
    }

    public class FacturaCompraElectronicaDto 
    {
        public int IdFacturaCompraElectronica { get; set; }
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string NumeroCompleto { get; set; }
        public string TipoDocumento { get; set; }
        public string FechaEmision { get; set; }
        public string? FechaVencimiento { get; set; }
        public string Moneda { get; set; }
        public decimal MontoBase { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal MontoTotal { get; set; }
        public string XmlOriginal { get; set; }
        public string CodigoHash { get; set; }

        public string RucEmpresa { get; set; }
    }
}
