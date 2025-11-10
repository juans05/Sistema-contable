using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Contador
{
    public  class ContadoresResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EContadorDto> Data { get; set; } = new();
        public List<string> Errors { get; set; } = new();
    }
}
