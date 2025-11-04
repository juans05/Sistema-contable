using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs
{
    public record UserSessionData
    {
        public int UsuarioId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public bool EsContador { get; init; }
        public bool EsSuperAdmin { get; init; }
        public Guid EmpresaId { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string Rol { get; init; } = string.Empty;
    }
}
