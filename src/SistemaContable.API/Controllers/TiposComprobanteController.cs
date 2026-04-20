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
    public class TiposComprobanteController : ControllerBase
    {
        private readonly ITipoComprobanteService _service;
        private readonly ILogger<TiposComprobanteController> _logger;

        public TiposComprobanteController(ITipoComprobanteService service, ILogger<TiposComprobanteController> logger)
        {
            _service = service;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<List<TipoComprobanteDto>>> Listar([FromQuery] bool? activo = null)
        {
            try
            {
                return Ok(await _service.ListarAsync(activo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar tipos de comprobante");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TipoComprobanteDto>> Obtener(int id)
        {
            try
            {
                var result = await _service.ObtenerPorIdAsync(id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener tipo comprobante");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearTipoComprobanteRequest request)
        {
            try
            {
                var id = await _service.CrearAsync(request);
                return CreatedAtAction(nameof(Obtener), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear tipo comprobante");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarTipoComprobanteRequest request)
        {
            try
            {
                var success = await _service.ActualizarAsync(id, request);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar tipo comprobante");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var success = await _service.EliminarAsync(id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar tipo comprobante");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}