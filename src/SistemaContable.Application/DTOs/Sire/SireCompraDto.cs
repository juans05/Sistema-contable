using System;

namespace SistemaContable.Application.DTOs.Sire
{
    public class SireCompraDto
    {
        public int Id { get; set; }
        public string RucEmpresa { get; set; }
        public string RazonSocialEmpresa { get; set; }
        public string Periodo { get; set; } // YYYYMM00
        public string Car { get; set; } // Código de Anotación de Registro
        public DateTime FechaEmision { get; set; }
        public DateTime? FechaVencimiento { get; set; }
        public string TipoComprobante { get; set; } // 01, 14, etc
        public string Serie { get; set; }
        public string AnioEmisionDua { get; set; } // Para DUA/DSI
        public string Numero { get; set; }
        public string TipoDocProveedor { get; set; } // 6
        public string RucProveedor { get; set; }
        public string RazonSocialProveedor { get; set; }
        
        // Importes
        public decimal BaseImponibleGravadaDG { get; set; } // Destino Gravado
        public decimal IgvDG { get; set; }
        
        public decimal BaseImponibleGravadaDM { get; set; } // Destino Mixto
        public decimal IgvDM { get; set; }
        
        public decimal BaseImponibleGravadaDNG { get; set; } // Destino No Gravado
        public decimal IgvDNG { get; set; }
        
        public decimal MontoExonerado { get; set; }
        public decimal MontoInafecto { get; set; }
        public decimal MontoIsc { get; set; }
        public decimal MontoIcbper { get; set; }
        public decimal OtrosTributos { get; set; }
        public decimal TotalComprobante { get; set; }
        
        public string Moneda { get; set; }
        public decimal TipoCambio { get; set; }
        
        // Referencias Detraccion
        public DateTime? FechaDetraccion { get; set; }
        public string NumeroConstanciaDetraccion { get; set; }
        
        // Referencias NC/ND
        public DateTime? FechaReferencia { get; set; }
        public string TipoReferencia { get; set; }
        public string SerieReferencia { get; set; }
        public string NumeroReferencia { get; set; }
    }
}
