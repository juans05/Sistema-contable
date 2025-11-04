using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class EmpresaDisponibleData
    {
        public Guid EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public string? EmpresaLogo { get; set; }
        public string Rol { get; set; } = string.Empty;
        public bool EsPrincipal { get; set; }
    }
}
