using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class CompraListaDto
    {
        public int IdRegCompras { get; set; }
        public string NumeroDocumento { get; set; }
        public DateTime FechaEmision { get; set; }
        public string RucProveedor { get; set; }
        public string NombreProveedor { get; set; }
        public string Moneda { get; set; }
        public decimal TotalDoc { get; set; }
        public string EstadoDoc { get; set; }
        public string EstadoSunat { get; set; }
        public string NumeroFactura { get; set; }
        public long CantidadItems { get; set; }
    }

    public class DetalleCompraDto
    {
        public int NumeroLinea { get; set; }
        public string CodigoProducto { get; set; }
        public string Descripcion { get; set; }
        public decimal Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Total { get; set; }
    }
}
