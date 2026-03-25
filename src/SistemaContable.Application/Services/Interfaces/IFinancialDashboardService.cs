using System.Threading.Tasks;
using SistemaContable.Application.DTOs.Dashboard;

namespace SistemaContable.Application.Services.Interfaces
{
    public interface IFinancialDashboardService
    {
        Task<FinancialSummaryDto> ObtenerResumenMensualAsync(string rucEmpresa, int mes, int anio);
    }
}
