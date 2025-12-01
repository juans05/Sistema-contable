using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces.IRepository
{
    public interface IVentaRepository
    {
        Task<ERegistroVenta> CrearConDetallesAsync(ERegistroVenta venta);
        Task<ERegistroVenta> ObtenerPorIdAsync(int id);
        Task<bool> ExisteVentaPorDocumentoAsync(string tipo, string serie, string numero);
    }
}
