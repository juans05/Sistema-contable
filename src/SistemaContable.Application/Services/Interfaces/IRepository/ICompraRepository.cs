using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface ICompraRepository
    {
        Task<bool> ExisteFacturaCompraPorHashAsync(string hash, string ruc);
        Task<SpResultado> InsertarCompraDetalleAsync(int idRegCompra, RegistroCompraDetalleDto compraDetalle);
        Task<SpResultado> InsertarFacturaCompraElectronicaAsync(FacturaCompraElectronicaDto facturaCompra, string usuario, string rucEmpresa);
        Task<SpResultado> InsertarRegistroCompraAsync(RegistroCompraDto compra, string usuario);
    }
}
