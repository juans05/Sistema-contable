using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class LoginData
    {
        public bool Success { get; set; }

        public int usuario_id { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string username { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string password_hash { get; set; } = string.Empty;
        public string Ruc { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;

        public Guid empresa_id { get; set; }
        public bool Activo { get; set; }
    }
    public record UsuarioDataResult
    {
        public int UsuarioId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public bool EsContador { get; init; }
        public bool EsSuperAdmin { get; init; }
        public bool Activo { get; init; }
    }

    public record EmpresaUsuarioResult
    {
        public Guid EmpresaId { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
        public string ruc { get; init; } = string.Empty;
    }
}
