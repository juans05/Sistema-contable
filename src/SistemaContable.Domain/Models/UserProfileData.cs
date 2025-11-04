using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class UserProfileData
    {
        public int UsuarioId { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
        public DateTime? UltimoAcceso { get; set; }

        public int PersonaId { get; set; }
        public string Nombres { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string NombreCompleto { get; set; } = string.Empty;
        public string? DocumentoTipo { get; set; }
        public string? DocumentoNumero { get; set; }
        public string? Telefono { get; set; }
        public string? FotoUrl { get; set; }

        public int Ruc { get; init; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string? EmpresaLogo { get; set; }
        public string Rol { get; set; } = string.Empty;
        public string? RolDescripcion { get; set; }

        public Dictionary<string, bool> Permisos { get; set; } = new();
        public List<MenuPermisoData> Menus { get; set; } = new();
        public List<EmpresaDisponibleData> EmpresasDisponibles { get; set; } = new();
        public ConfiguracionData Configuracion { get; set; } = new();
    }
}
