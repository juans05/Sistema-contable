namespace SistemaContable.Domain.Models
{
    public class CrearMarcaRequest
    {
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Origen { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ActualizarMarcaRequest : CrearMarcaRequest
    {
    }
}