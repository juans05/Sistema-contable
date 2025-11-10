using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.Empresa
{
    public class CambiarEstadoRequest
    {
        [Required(ErrorMessage = "El ID de la empresa es requerido")]
        public Guid EmpresaId { get; set; }

        [Required(ErrorMessage = "El estado es requerido")]
        public bool Activo { get; set; }
    }
}
