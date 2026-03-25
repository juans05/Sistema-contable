using Microsoft.AspNetCore.Http;
using SistemaContable.Application.DTOs.Responses.Venta;
using SistemaContable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ICompraService
    {
        Task<ProcesarXmlCompraRespondeDto> ProcesarXmlYRegistrarCompraAsync(List<IFormFile> archivosXml, string usuario, string ruc);

        Task<VentaCompletaDto> ObtenerCompraPorIdAsync(int idRegVenta);
        Task<List<VentaListaDto>> ListarCompraAsync(
            string fechaDesde, string fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null,
            int page = 1, int pageSize = 10, string filtro = null);
        Task<AnularVentaResponseDTO> AnularCompraAsync(int idRegVenta, string motivo, string usuario);
        Task<string> ObtenerXmlCompraAsync(int idRegVenta);
        Task<byte[]> GenerarReporteExcelComprasAsync(
            string fechaDesde, string fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null);
    }
}
