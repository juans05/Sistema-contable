using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ICompraService
    {
        Task<ProcesarXmlCompraRespondeDto> ProcesarXmlYRegistrarCompraAsync(List<IFormFile> archivosXml, string usuario, string ruc);
    }
}
