using System;

namespace SistemaContable.Domain.Entities
{
    public class EEventoContableTipo
    {
        public int Id { get; set; }
        public string CodigoEvento { get; set; } // VENTA_MERCADERIA, ETC.
        public string Descripcion { get; set; }
        public string ModuloOrigen { get; set; } // VENTAS, COMPRAS
    }
}
