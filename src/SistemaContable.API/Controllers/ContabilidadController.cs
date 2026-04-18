using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;

namespace SistemaContable.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ContabilidadController : ControllerBase
    {
        private readonly IAccountingEngineService _accountingEngine;

        private readonly IAccountingRepository _repo;
        private readonly ITokenDataService _tokenDataService;

        public ContabilidadController(IAccountingEngineService accountingEngine, IAccountingRepository repo, ITokenDataService tokenDataService)
        {
            _accountingEngine = accountingEngine;
            _repo = repo;
            _tokenDataService = tokenDataService;
        }

        [HttpPost("generar-asiento-venta/{idVenta}")]
        public async Task<IActionResult> GenerarAsientoVenta(int idVenta)
        {
            var asientoId = await _accountingEngine.GenerarAsientoVentaAsync(idVenta);
            
            if (asientoId > 0)
                return Ok(new { AsientoId = asientoId, Mensaje = "Asiento generado correctamente" });
            
            return BadRequest(new { Mensaje = "No se pudo generar el asiento. Verifique reglas contables." });
        }

        [HttpPost("generar-asiento-compra/{idCompra}")]
        public async Task<IActionResult> GenerarAsientoCompra(int idCompra)
        {
            var asientoId = await _accountingEngine.GenerarAsientoCompraAsync(idCompra);
            
            if (asientoId > 0)
                return Ok(new { AsientoId = asientoId, Mensaje = "Asiento y Destino generados correctamente" });
            
            return BadRequest(new { Mensaje = "No se pudo generar el asiento de compra." });
        }

        // ===== CONFIGURACIÓN =====

        [HttpGet("eventos")]
        public async Task<IActionResult> ListarEventos()
        {
            var eventos = await _repo.ListarEventosDisponiblesAsync();
            return Ok(eventos);
        }

        [HttpGet("reglas/{codigoEvento}")]
        public async Task<IActionResult> ListarReglas(string codigoEvento)
        {
            try
            {
                // FIXME: Obtener real del token
                int empresaId = _tokenDataService.GetEmpresaId();
                if (empresaId == 0)
                    return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });
                var reglas = await _repo.ObtenerReglasPorEventoAsync(codigoEvento, empresaId);
                return Ok(reglas);

            }
            catch (Exception ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
        }

        [HttpPost("reglas")]
        public async Task<IActionResult> GuardarRegla([FromBody] EReglaContable regla)
        {
            // FIXME: Obtener real del token
            regla.EmpresaId = _tokenDataService.GetEmpresaId(); 
            if (regla.EmpresaId == 0)
                return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });

            var success = await _repo.GuardarReglaAsync(regla);
            return success ? Ok() : BadRequest("No se pudo guardar la regla");
        }

        [HttpDelete("reglas/{id}")]
        public async Task<IActionResult> EliminarRegla(int id)
        {
            try
            {
                int empresaId = _tokenDataService.GetEmpresaId();
                if (empresaId == 0)
                    return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });
                var success = await _repo.EliminarReglaAsync(id, empresaId);
                return success ? Ok() : BadRequest("No se pudo eliminar la regla");
            }
            catch (System.Exception ex)
            {
                return Unauthorized(new { Mensaje = ex.Message });
            }
        }

        [HttpPost("importar-plan")]
        public async Task<IActionResult> ImportarPlan(Microsoft.AspNetCore.Http.IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No se ha enviado ningún archivo.");

            try
            {
                int empresaId = _tokenDataService.GetEmpresaId();
                
                using var stream = file.OpenReadStream();
                var result = await _accountingEngine.ImportarPlanCuentasExcelAsync(stream, empresaId);
                
                return result ? Ok(new { Mensaje = "Importación exitosa" }) : BadRequest("Falló la importación");
            }
            catch(System.Exception ex)
            {
                return StatusCode(500, new { Mensaje = ex.Message });
            }
        }

        // ===== CRUD MANUAL =====

        [HttpGet("plan")]
        public async Task<IActionResult> ListarPlan([FromQuery] string? busqueda)
        {
            // FIXME: Token
            int empresaId = _tokenDataService.GetEmpresaId();
            if (empresaId == 0)
                return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });

            // _logger is needed. It's not injected in the constructor in the previous view, let's check.
            // Ah, I need to check if _logger is available. Listing view_file suggests it wasn't.
            // I will assume I need to adding Logger injection.

            System.Console.WriteLine($"[API] ListarPlan solicitado. Busqueda: {busqueda}");
            
            var lista = await _repo.ListarPlanCuentasAsync(empresaId, busqueda);
            
             System.Console.WriteLine($"[API] ListarPlan recuperó {lista?.Count} registros.");
            
            return Ok(lista);
        }

        [HttpPost("plan")]
        public async Task<IActionResult> GuardarCuenta([FromBody] EPlanContable cuenta)
        {
            try 
            {
                int empresaId = _tokenDataService.GetEmpresaId();
                if (empresaId == 0)
                    return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });
                cuenta.EmpresaId = empresaId;
                
                var success = await _repo.GuardarCuentaAsync(cuenta);
                return success ? Ok() : BadRequest("No se pudo guardar la cuenta");
            }
            catch(System.Exception ex)
            {
                return BadRequest(new { Mensaje = ex.Message });
            }
        }

        [HttpDelete("plan/{id}")]
        public async Task<IActionResult> EliminarCuenta(int id)
        {
            int empresaId = _tokenDataService.GetEmpresaId();
            if (empresaId == 0)
                return Unauthorized(new { Mensaje = "Empresa no identificada en el token" });
            var success = await _repo.EliminarCuentaAsync(id, empresaId);
            return success ? Ok() : BadRequest("No se pudo eliminar (o no existe/permisos)");
        }
    }
}
