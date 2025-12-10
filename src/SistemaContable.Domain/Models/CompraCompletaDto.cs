using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class CompraCompletaDto
    {
        public int IdRegCompras { get; set; }
        public int? IdFacturaCompraElectronica { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RucProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string Moneda { get; set; }
        public decimal SubTotal { get; set; }
        public decimal ImpIgv { get; set; }
        public decimal TotalDoc { get; set; }
        public string EstadoDoc { get; set; }
        public string EstadoSunat { get; set; }
        public string NumeroFacturaCompraElectronica { get; set; }
        public List<DetalleCompraDto> Detalles { get; set; }
    }
}
