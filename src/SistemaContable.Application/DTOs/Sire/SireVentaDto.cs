using System;

namespace SistemaContable.Application.DTOs.Sire
{
    public class SireVentaDto
    {
        public int Id { get; set; }
        public string RucEmpresa { get; set; }
        public string RazonSocialEmpresa { get; set; }
        public string Periodo { get; set; } // YYYYMM00
        public string Car { get; set; } // Código de Anotación de Registro (puede ser vacío en reemplazo)
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string TipoComprobante { get; set; } // 01, 03, 07, 08
        public string Serie { get; set; }
        public string Numero { get; set; }
        public string TipoDocCliente { get; set; } // 6, 1, 0
        public string RucCliente { get; set; }
        public string RazonSocialCliente { get; set; }
        public decimal ValoFacturadoExportacion { get; set; }
        public decimal BaseImponibleGravada { get; set; }
        public decimal MontoIgv { get; set; }
        public decimal MontoExonerado { get; set; }
        public decimal MontoInafecto { get; set; }
        public decimal MontoIsc { get; set; }
        public decimal MontoIcbper { get; set; }
        public decimal OtrosTributos { get; set; }
        public decimal TotalComprobante { get; set; }
        public string Moneda { get; set; }
        public decimal TipoCambio { get; set; }
        
        // Datos para notas de crédito/débito
        public DateTime? FechaReferencia { get; set; }
        public string TipoReferencia { get; set; }
        public string SerieReferencia { get; set; }
        public string NumeroReferencia { get; set; }
    }
}
