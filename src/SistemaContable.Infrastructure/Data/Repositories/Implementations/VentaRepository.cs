using SistemaContable.Domain.Entities;
using SistemaContable.Infrastructure.Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class VentaRepository : IVentaRepository
    {
        public Task<ERegistroVenta> CrearConDetallesAsync(RegistroVenta venta)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExisteVentaPorDocumentoAsync(string tipo, string serie, string numero)
        {
            throw new NotImplementedException();
        }

        public Task<ERegistroVenta> ObtenerPorIdAsync(int id)
        {
            throw new NotImplementedException();
        }
    }
}
