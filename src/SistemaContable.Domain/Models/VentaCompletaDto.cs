using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class VentaCompletaDto
    {
        public int IdRegVenta { get; set; }
        public int? IdFacturaElectronica { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RucCliente { get; set; }
        public string RazonSocialCliente { get; set; }
        public string Moneda { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ImpIgv { get; set; }
        public decimal TotalDoc { get; set; }
        public string EstadoDoc { get; set; }
        public string EstadoSunat { get; set; }
        public string NumeroFacturaElectronica { get; set; }
        public List<DetalleVentaDto> Detalles { get; set; }
    }
}
