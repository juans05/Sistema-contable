using System;

namespace SistemaContable.Domain.Entities
{
    public class Moneda
    {
        public int Id { get; set; }
        public string Codigo { get; set; } // PEN, USD
        public string Nombre { get; set; }
        public string Simbolo { get; set; }
        public bool Activo { get; set; }
    }
}
