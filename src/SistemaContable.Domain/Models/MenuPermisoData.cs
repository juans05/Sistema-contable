using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class MenuPermisoData
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
    }
}
