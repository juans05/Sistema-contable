using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Empresa
{
    public class CreateEmpresaResponse
    {
        public bool Success { get; set; } = true;
        public string Message { get; set; } = "Empresa creada exitosamente";
        public Guid EmpresaId { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
