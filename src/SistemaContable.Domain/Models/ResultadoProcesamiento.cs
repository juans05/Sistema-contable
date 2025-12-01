using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class ResultadoProcesamiento
    {
        public string NombreArchivo { get; set; }
        public string NumeroDocumento { get; set; }
        public bool Procesado { get; set; }
        public string Error { get; set; }
        public int IdVenta { get; set; }
        public int IdFacturaElectronica { get; set; }
    }
}
