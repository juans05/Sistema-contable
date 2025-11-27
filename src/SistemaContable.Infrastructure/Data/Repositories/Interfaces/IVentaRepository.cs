using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Interfaces
{
    public  interface IVentaRepository
    {
        Task<ERegistroVenta> CrearConDetallesAsync(RegistroVenta venta);
        Task<ERegistroVenta> ObtenerPorIdAsync(int id);
        Task<bool> ExisteVentaPorDocumentoAsync(string tipo, string serie, string numero);
    }
}
