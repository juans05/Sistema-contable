using Microsoft.AspNetCore.Http;
using SistemaContable.Application.DTOs.Responses.XML;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public  interface IVentaElectronicaService
    {
        Task<ProcesarXmlVentaResponseDto> ProcesarXmlYRegistrarVentaAsync(
            List<IFormFile> archivosXml, string usuario);
    }
}
