using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class ProcesarXmlCompraRespondeDto
    {
        public bool Exito { get; set; }
        public string Mensaje { get; set; }
        public int ComprasRegistradas { get; set; }

        public List<ResultadoCompraProcesamiento> Resultados { get; set; }
    }

    public class ResultadoCompraProcesamiento 
    {
        public string NombreArchivo { get; set; }
        public string NumeroDocumento { get; set; }
        public bool Procesado { get; set; }
        public string Error { get; set; }
        public int IdCompra { get; set; }
        public int IdFacturaCompraElectronica { get; set; }
    }
}
