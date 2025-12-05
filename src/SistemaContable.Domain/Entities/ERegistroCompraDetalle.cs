using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class ERegistroCompraDetalle
    {
        public int IdDetalle { get; set; }
        public int IdRegCompras { get; set; }
        public int NumeroLinea { get; set; }
        public string Categoria { get; set; }
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

        public virtual ERegistroCompra RegistroCompra { get; set; }
    }
}
