using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class EFacturaCompraElectronica
    {
        public int IdFacturaCompraElectronica { get; set; }
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
        public decimal MontoDescuento { get; set; }
        public string EstadoSunat { get; set; }
        public string CodigoHash { get; set; }
        public string XmlOriginal { get; set; }
        public string XmlFirmado { get; set; }
        public string CdrSunat { get; set; }
        public DateTime CreatedAt { get; set; }

        public virtual ICollection<ERegistroCompra> Compras { get; set; }
    }
}
