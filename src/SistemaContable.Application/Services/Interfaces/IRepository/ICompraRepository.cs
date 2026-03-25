using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface ICompraRepository
    {
        Task<bool> ExisteFacturaCompraPorHashAsync(string hash, string ruc);
        Task<SpResultado> InsertarCompraDetalleAsync(int idRegCompra, RegistroCompraDetalleDto compraDetalle);
        Task<SpResultado> InsertarFacturaCompraElectronicaAsync(FacturaCompraElectronicaDto facturaCompra, string usuario, string rucEmpresa);
        Task<SpResultado> InsertarRegistroCompraAsync(RegistroCompraDto compra, string usuario);
        Task<List<VentaListaDto>> ListarComprasAsync(
           string fechaDesde, string fechaHasta,
           string rucProveedor = null, string tipoDoc = null,
           string estadoDoc = null, string _RucEmpresa = null,
           int limit = 10, int offset = 0, string filtro = null);
        Task<VentaCompletaDto> ObtenerCompraCompletaAsync(int idRegCompra);
        Task<ERegistroCompra> ObtenerCompraPorIdAsync(int idRegCompra);
        Task<string> ObtenerXmlCompraPorIdAsync(int idRegCompra);
        Task<List<SistemaContable.Application.DTOs.Sire.SireCompraDto>> ListarComprasParaSire(string periodo, string rucEmpresa);
    }
}
