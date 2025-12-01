using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class ERegistroVenta
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
        public string CodDetrac { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ImpIgv { get; set; }
        public decimal TotalDoc { get; set; }
        public decimal? MontoDetrac { get; set; }
        public decimal? PorcDetrac { get; set; }
        public bool AplDetrac { get; set; }
        public string EstadoDoc { get; set; }
        public string TipOperaSunat { get; set; }
        public DateTime CreatedAt { get; set; }

        public string UsuarioCreacion { get; set; }

        // Navegación
        public virtual EFacturaElectronica FacturaElectronica { get; set; }
        public virtual ICollection<ERegistroVentaDetalle> Detalles { get; set; }
    }
}
