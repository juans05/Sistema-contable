using System;

namespace SistemaContable.Domain.Entities
{
    public class EProductoCategoria
    {
        public int IdCategoria { get; set; }
        public string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class EProductoMarca
    {
        public int IdMarca { get; set; }
        public string Nombre { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }

    public class EProductoUnidadMedida
    {
        public int IdUnidad { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string? Simbolo { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
