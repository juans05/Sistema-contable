using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using SistemaContable.Application.DTOs.Responses.Compra;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface ICompraService
    {
        Task<ProcesarXmlCompraRespondeDto> ProcesarXmlYRegistrarCompraAsync(List<IFormFile> archivosXml, string usuario, string ruc);

        Task<List<CompraListaDto>> ListarComprasAsync(
            string fechaDesde, string fechaHasta,
            string rucProveedor = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null);

        Task<AnularCompraResponseDTO> AnularCompraAsync(int idRegCompras, string motivo, string usuario);

        Task<CompraCompletaDto> ObtenerCompraPorIdAsync(int idRegCompras);
    }
}
