using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.Compra
{
    public class AnularCompraRequest
    {
        public string Motivo { get; set; } = string.Empty;
    }
}
