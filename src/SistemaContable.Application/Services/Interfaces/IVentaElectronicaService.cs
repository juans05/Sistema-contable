using Microsoft.AspNetCore.Http;
using SistemaContable.Application.DTOs.Responses.XML;
using SistemaContable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public  interface IVentaElectronicaService
    {
        Task<ProcesarXmlResponseDto> ProcesarXmlYRegistrarVentaAsync(
           List<IFormFile> archivosXml, string usuario);
        Task<VentaCompletaDto> ObtenerVentaPorIdAsync(int idRegVenta);
        Task<List<VentaListaDto>> ListarVentasAsync(
            DateTime fechaDesde, DateTime fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null);
    }
}
