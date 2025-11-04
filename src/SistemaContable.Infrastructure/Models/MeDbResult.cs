using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Models
{
    public class MeDbResult
    {
        public int usuario_id { get; init; }
        public string email { get; init; } = string.Empty;
        public string username { get; init; } = string.Empty;
        public string nombre_completo { get; init; } = string.Empty;
        public string? telefono { get; init; }
        public string? avatar_url { get; init; }
        public bool es_contador { get; init; }
        public bool es_super_admin { get; init; }
        public bool activo { get; init; }
        public bool EmailVerificado { get; init; }
        public DateTime fecha_creacion { get; init; }
        public DateTime? ultimo_acceso { get; init; }

        public Guid empresa_id { get; init; }
        public string empresa_nombre { get; init; } = string.Empty;

        public string? empresa_razon_social { get; init; }
        public string? empresa_ruc { get; init; }
        public string? empresa_logo { get; init; }
        public string Rol { get; init; } = string.Empty;
        public string? RolDescripcion { get; init; }

        // JSONB
        public string? Permisos { get; init; }
        public string? Menus { get; init; }
        public string? EmpresasDisponibles { get; init; }
        public string? Configuracion { get; init; }
    }
}
