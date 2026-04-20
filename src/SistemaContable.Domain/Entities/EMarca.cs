using System;

namespace SistemaContable.Domain.Entities
{
    public class EMarca
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Origen { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}