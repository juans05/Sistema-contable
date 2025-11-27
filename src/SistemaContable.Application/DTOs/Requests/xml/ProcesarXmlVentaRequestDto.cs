using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.xml
{
    public class ProcesarXmlVentaRequestDto
    {
        public List<IFormFile> ArchivosXml { get; set; }
        public string UsuarioRegistro { get; set; }
        public bool GenerarAsientoContable { get; set; } = false;
    }
}
