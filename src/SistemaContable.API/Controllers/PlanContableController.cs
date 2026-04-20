using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PlanContableController : ControllerBase
    {
        private readonly IPlanContableService _service;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly ILogger<PlanContableController> _logger;

        public PlanContableController(IPlanContableService service, IRucEmpresaService rucEmpresaService, ILogger<PlanContableController> logger)
        {
            _service = service;
            _rucEmpresaService = rucEmpresaService;
            _logger = logger;
        }

        private string RucEmpresa => _rucEmpresaService.ObtenerRucActual() 
            ?? throw new UnauthorizedAccessException("RUC de empresa no encontrado en el token");

        [HttpGet]
        public async Task<ActionResult<List<PlanContableDetalleDto>>> Listar([FromQuery] bool? activo = null)
        {
            try
            {
                return Ok(await _service.ListarAsync(RucEmpresa, activo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar plan contable");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PlanContableDetalleDto>> Obtener(int id)
        {
            try
            {
                var result = await _service.ObtenerPorIdAsync(RucEmpresa, id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener plan contable");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearPlanContableRequest request)
        {
            try
            {
                var id = await _service.CrearAsync(RucEmpresa, request);
                return CreatedAtAction(nameof(Obtener), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear cuenta contable");
                return StatusCode(500, new { mensaje = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarPlanContableRequest request)
        {
            try
            {
                var success = await _service.ActualizarAsync(RucEmpresa, id, request);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar cuenta contable");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var success = await _service.EliminarAsync(RucEmpresa, id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar cuenta contable");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}