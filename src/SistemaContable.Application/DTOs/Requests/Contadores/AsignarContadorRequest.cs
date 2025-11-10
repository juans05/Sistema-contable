using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.Contadores
{
    public class AsignarContadorRequest
    {
        [Required(ErrorMessage = "El ID de la empresa es requerido")]
        public Guid EmpresaId { get; set; }

        [Required(ErrorMessage = "El ID del contador es requerido")]
        [Range(1, int.MaxValue, ErrorMessage = "El ID del contador debe ser mayor a 0")]
        public int ContadorId { get; set; }

        public bool PuedeCrearUsuarios { get; set; } = false;

        public bool PuedeModificarConfig { get; set; } = false;

        public bool PuedeEliminar { get; set; } = false;
    }
}
