using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses
{
    public class UserResponse
    {
        public int UsuarioId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public bool Activo { get; init; }
        public DateTime FechaCreacion { get; init; }
        public DateTime? UltimoAcceso { get; init; }

        // Persona
        public int PersonaId { get; init; }
        public string Nombres { get; init; } = string.Empty;
        public string Apellidos { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public string? DocumentoTipo { get; init; }
        public string? DocumentoNumero { get; init; }
        public string? Telefono { get; init; }
        public string? FotoUrl { get; init; }

        // Empresa y Rol
        public Guid EmpresaId { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string? EmpresaLogo { get; init; }
        public string Rol { get; init; } = string.Empty;
        public string? RolDescripcion { get; init; }

        // Permisos
        public Dictionary<string, bool> Permisos { get; init; } = new();
        public List<MenuPermiso> Menus { get; init; } = new();
        public List<EmpresaDisponible> EmpresasDisponibles { get; init; } = new();
        public ConfiguracionUsuario Configuracion { get; init; } = new();
    }

    public record MenuPermiso
    {
        public int MenuId { get; init; }
        public string MenuKey { get; init; } = string.Empty;
        public string Titulo { get; init; } = string.Empty;
        public string? Descripcion { get; init; }
        public string? Icono { get; init; }
        public int? ParentId { get; init; }
        public int Orden { get; init; }
        public string? Ruta { get; init; }
        public string Tipo { get; init; } = string.Empty;
        public bool PuedeVer { get; init; }
        public bool PuedeCrear { get; init; }
        public bool PuedeEditar { get; init; }
        public bool PuedeEliminar { get; init; }
        public bool PuedeExportar { get; init; }
        public bool EsFavorito { get; init; }
        public Dictionary<string, object>? Metadata { get; init; }
    }

    public record EmpresaDisponible
    {
        public Guid EmpresaId { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string? EmpresaLogo { get; init; }
        public string Rol { get; init; } = string.Empty;
        public bool EsPrincipal { get; init; }
    }

    public record ConfiguracionUsuario
    {
        public string Tema { get; init; } = "light";
        public string Idioma { get; init; } = "es";
        public bool NotificacionesEmail { get; init; } = true;
        public bool NotificacionesPush { get; init; } = true;
        public string Timezone { get; init; } = "America/Lima";
    }
}
