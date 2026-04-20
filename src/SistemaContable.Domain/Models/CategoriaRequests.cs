namespace SistemaContable.Domain.Models
{
    public class CrearCategoriaRequest
    {
        public string? Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int? CategoriaPadreId { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ActualizarCategoriaRequest : CrearCategoriaRequest
    {
    }
}