using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.XML
{
    public class ProcesarXmlVentaResponseDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int VentasRegistradas { get; set; }
        public List<ResultadoProcesamiento> Resultados { get; set; }
    }
}
