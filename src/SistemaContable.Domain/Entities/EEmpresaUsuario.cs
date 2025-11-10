using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public  class EEmpresaUsuario
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public Guid EmpresaId { get; set; }
        public bool PuedeCrearUsuarios { get; set; }
        public bool PuedeModificarConfig { get; set; }
        public bool PuedeEliminar { get; set; }
        public DateTime FechaAsignacion { get; set; }
        public int AsignadoPor { get; set; }
        public bool Activo { get; set; }

        // Navegación
        public EUsuario Usuario { get; set; } = null!;
        public EEmpresa Empresa { get; set; } = null!;
    }
}
