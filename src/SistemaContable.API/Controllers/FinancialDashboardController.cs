using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SistemaContable.Application.Services.Interfaces;
using System;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinancialDashboardController : ControllerBase
    {
        private readonly IFinancialDashboardService _dashboardService;

        public FinancialDashboardController(IFinancialDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int mes, [FromQuery] int anio)
        {
            string rucEmpresa = GetRucEmpresa();
            try
            {
                var summary = await _dashboardService.ObtenerResumenMensualAsync(rucEmpresa, mes, anio);
                return Ok(summary);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error dashboard: {ex.Message}");
            }
        }

        private string GetRucEmpresa()
        {
            if (HttpContext.Items.TryGetValue("RucEmpresa", out var rucObj) && rucObj is string ruc)
            {
                return ruc;
            }
            // Fallback development
            return "20600000001";
        }
    }
}
