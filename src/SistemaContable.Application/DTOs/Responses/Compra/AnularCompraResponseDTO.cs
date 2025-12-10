using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Compra
{
    public class AnularCompraResponseDTO
    {
        public bool Anulado { get; set; } = true;

        public string Message { get; set; } = string.Empty;
    }
}
