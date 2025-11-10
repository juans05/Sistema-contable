using SistemaContable.Application.DTOs.Common;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Empresa
{
    public class PagedEmpresaResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public PagedResultDto<EEmpresa>? Data { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
