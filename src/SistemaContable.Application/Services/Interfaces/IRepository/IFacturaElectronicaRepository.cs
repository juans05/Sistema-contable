using SistemaContable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IFacturaElectronicaRepository
    {
        Task<SpResultado> InsertarFacturaElectronicaAsync(FacturaElectronicaDto factura, string usuario);
        Task<SpResultado> InsertarRegistroVentaAsync(RegistroVentaDto venta, string usuario);
        Task<SpResultado> InsertarVentaDetalleAsync(int idRegVenta, RegistroVentaDetalleDto detalle);
        Task<bool> VerificarDuplicadoHashAsync(string hash);
        Task<VentaCompletaDto> ObtenerVentaCompletaAsync(int idRegVenta);
        Task<List<VentaListaDto>> ListarVentasAsync(
            DateTime fechaDesde, DateTime fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, int limite = 100, int offset = 0);
        Task<SpResultado> ActualizarEstadoSunatAsync(
            int idFactura, string estado, string codigo, string mensaje,
            string cdr = null, string xmlFirmado = null);
        Task<SpResultado> AnularVentaAsync(int idRegVenta, string motivo, string usuario);
        Task<bool> ExisteFacturaPorHashAsync(string hash);

        Task<FacturaElectronicaDto> CrearAsync(FacturaElectronicaDto factura, string usuario);


    }
}
