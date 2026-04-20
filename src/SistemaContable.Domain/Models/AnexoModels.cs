namespace SistemaContable.Domain.Models
{
    public class AnexoDto
    {
        public int Id { get; set; }
        public string TipoAnexo { get; set; } = string.Empty;
        public string CodigoAnexo { get; set; } = string.Empty;
        public string TipoDocumentoId { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string TipoPersona { get; set; } = string.Empty;
        public string? RazonSocial { get; set; }
        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        public string NombreCompleto => TipoPersona == "02" 
            ? RazonSocial ?? string.Empty 
            : $"{Nombres} {ApellidoPaterno} {ApellidoMaterno}".Trim();
        public string? Sexo { get; set; }
        public string? Nacionalidad { get; set; }
        public string? Direccion { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearAnexoRequest
    {
        public string TipoAnexo { get; set; } = string.Empty;
        public string CodigoAnexo { get; set; } = string.Empty;
        public string TipoDocumentoId { get; set; } = string.Empty;
        public string NumeroDocumento { get; set; } = string.Empty;
        public string TipoPersona { get; set; } = string.Empty;
        
        public string? RazonSocial { get; set; }
        public string? Nombres { get; set; }
        public string? ApellidoPaterno { get; set; }
        public string? ApellidoMaterno { get; set; }
        
        public string? Sexo { get; set; }
        public string? Nacionalidad { get; set; }
        public string? Direccion { get; set; }
        public string? Correo { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ActualizarAnexoRequest : CrearAnexoRequest
    {
    }
}