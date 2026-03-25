using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using SistemaContable.Application.Services.Interfaces;
using System;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SireController : ControllerBase
    {
        private readonly ISireService _sireService;

        public SireController(ISireService sireService)
        {
            _sireService = sireService;
        }

        private string GetRucEmpresa()
        {
            if (HttpContext.Items.TryGetValue("RucEmpresa", out var rucObj) && rucObj is string ruc)
            {
                return ruc;
            }
            // Fallback para desarrollo o si falla middleware (aunque debería estar protegido)
            return "20600000001";
        }

        [HttpGet("rvie/{periodo}")]
        public async Task<IActionResult> DescargarRvie(string periodo)
        {
            string rucEmpresa = GetRucEmpresa();
            
            try 
            {
                var zipBytes = await _sireService.GenerarRvieReemplazoAsync(periodo, rucEmpresa);
                var fileName = $"RVIE_{rucEmpresa}_{periodo}.zip"; // Nombre más descriptivo
                
                return File(zipBytes, "application/zip", fileName);
            }
            catch(Exception ex)
            {
                return BadRequest($"Error generando SIRE: {ex.Message}");
            }
        }

        [HttpGet("rce/{periodo}")]
        public async Task<IActionResult> DescargarRce(string periodo)
        {
            string rucEmpresa = GetRucEmpresa();
            try 
            {
                var zipBytes = await _sireService.GenerarRceReemplazoAsync(periodo, rucEmpresa);
                var fileName = $"RCE_{rucEmpresa}_{periodo}.zip";
                
                return File(zipBytes, "application/zip", fileName);
            }
            catch(Exception ex)
            {
                return BadRequest($"Error generando SIRE Compras: {ex.Message}");
            }
        }
    }
}
