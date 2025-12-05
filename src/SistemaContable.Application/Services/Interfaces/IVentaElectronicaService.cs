using Microsoft.AspNetCore.Http;
using SistemaContable.Application.DTOs.Responses.Venta;
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
           List<IFormFile> archivosXml, string usuario, string rucEmpresa);
        Task<VentaCompletaDto> ObtenerVentaPorIdAsync(int idRegVenta);
        Task<List<VentaListaDto>> ListarVentasAsync(
            string fechaDesde, string fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null);
        Task<AnularVentaResponseDTO> AnularVentaAsync(int idRegVenta, string motivo, string usuario);
    }
}
