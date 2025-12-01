using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class DetalleVentaDto
    {
        public int NumeroLinea { get; set; }
        public string CodigoProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }

    public class VentaListaDto
    {
        public int IdRegVenta { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RucCliente { get; set; }
        public string RazonSocial { get; set; }
        public string Moneda { get; set; }
        public decimal TotalDoc { get; set; }
        public string EstadoDoc { get; set; }
        public string EstadoSunat { get; set; }
        public string NumeroFactura { get; set; }
        public long CantidadItems { get; set; }
    }
}
