using System;

namespace SistemaContable.Domain.Entities
{
    public class ETipoComprobante
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? CodigoSunat { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}