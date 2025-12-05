using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Venta
{
    public class AnularVentaResponseDTO
    {
        public bool Anulado { get; set; } = true;

        public string Message { get; set; } = string.Empty;
    }
}
