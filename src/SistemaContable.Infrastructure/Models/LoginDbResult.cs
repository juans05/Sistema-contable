using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Models
{
    public  class LoginDbResult
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;

        public int usuario_id { get; set; }
        public string username { get; init; } = string.Empty;
        public string nombre_completo { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public string password_hash { get; set; } = string.Empty;
        public string? telefono { get; init; }
        public string? avatar_url { get; init; }
        public bool es_contador { get; init; }
        public bool es_super_admin { get; init; }
        public string empresa_ruc { get; init; }

        public Guid empresa_id { get; init; }
        public string empresa_nombre { get; init; } = string.Empty;
        public string rol { get; init; } = string.Empty;
        public bool activo { get; init; }
    }
}
