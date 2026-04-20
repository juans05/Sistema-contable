using System;

namespace SistemaContable.Domain.Entities
{
    public class EAnexo
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
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
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}