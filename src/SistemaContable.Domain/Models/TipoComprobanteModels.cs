namespace SistemaContable.Domain.Models
{
    public class TipoComprobanteDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? CodigoSunat { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearTipoComprobanteRequest
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public string? CodigoSunat { get; set; }
        public string? Tipo { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ActualizarTipoComprobanteRequest : CrearTipoComprobanteRequest
    {
    }
}