using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IRucEmpresaService
    {
        string ObtenerRucActual();
        void EstablecerRuc(string ruc);
    }
}
