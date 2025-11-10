using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Empresa
{
    public class CambiarEstadoResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Guid EmpresaId { get; set; }
        public bool NuevoEstado { get; set; }
    }
}
