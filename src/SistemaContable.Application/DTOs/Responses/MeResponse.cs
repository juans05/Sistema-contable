using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses
{
    public  class MeResponse
    {
        public int UsuarioId { get; init; }
        public string Email { get; init; } = string.Empty;
        public string Username { get; init; } = string.Empty;
        public string NombreCompleto { get; init; } = string.Empty;
        public string? Telefono { get; init; }
        public string? AvatarUrl { get; init; }
        public bool EsContador { get; init; }
        public bool EsSuperAdmin { get; init; }
        public bool Activo { get; init; }
        public bool EmailVerificado { get; init; }
        public DateTime FechaCreacion { get; init; }
        public DateTime? UltimoAcceso { get; init; }

        // Empresa y Rol
        public string Ruc { get; init; }
        public string EmpresaNombre { get; init; } = string.Empty;
        public string? EmpresaRuc { get; init; }
        public string? EmpresaLogo { get; init; }
        public string Rol { get; init; } = string.Empty;
        public string? RolDescripcion { get; init; }

        // Permisos y Menús
        public Dictionary<string, bool> Permisos { get; init; } = new();
        public List<MenuPermiso> Menus { get; init; } = new();
        public List<EmpresaDisponible> EmpresasDisponibles { get; init; } = new();
        public ConfiguracionUsuario Configuracion { get; init; } = new();
    }

    public class MenuData
    {
        public int MenuId { get; set; }
        public string MenuKey { get; set; } = string.Empty;
        public string Titulo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public string? Icono { get; set; }
        public int? ParentId { get; set; }
        public int Orden { get; set; }
        public string? Ruta { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public bool PuedeVer { get; set; }
        public bool PuedeCrear { get; set; }
        public bool PuedeEditar { get; set; }
        public bool PuedeEliminar { get; set; }
        public bool PuedeExportar { get; set; }
        public bool EsFavorito { get; set; }
        public Dictionary<string, object>? Metadata { get; set; }
    }

    public class EmpresaData
    {
        public Guid EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string? EmpresaLogo { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }

    public class ConfiguracionData
    {
        public string Tema { get; set; } = "light";
        public string Idioma { get; set; } = "es";
        public bool NotificacionesEmail { get; set; } = true;
        public bool NotificacionesPush { get; set; } = true;
        public string Timezone { get; set; } = "America/Lima";
    }
}
