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
    public class MarcasController : ControllerBase
    {
        private readonly IMarcaService _marcaService;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly ILogger<MarcasController> _logger;

        public MarcasController(
            IMarcaService marcaService, 
            IRucEmpresaService rucEmpresaService, 
            ILogger<MarcasController> logger)
        {
            _marcaService = marcaService;
            _rucEmpresaService = rucEmpresaService;
            _logger = logger;
        }

        private string RucEmpresa => _rucEmpresaService.ObtenerRucActual() 
            ?? throw new UnauthorizedAccessException("RUC de empresa no encontrado en el token");

        [HttpGet]
        [ProducesResponseType(typeof(List<MarcaDto>), 200)]
        public async Task<IActionResult> Listar([FromQuery] bool? activo = null)
        {
            try
            {
                var result = await _marcaService.ListarAsync(RucEmpresa, activo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar marcas");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(MarcaDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Obtener(int id)
        {
            try
            {
                var result = await _marcaService.ObtenerPorIdAsync(RucEmpresa, id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener marca");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Crear([FromBody] CrearMarcaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Nombre))
                    return BadRequest(new { mensaje = "El nombre es requerido" });

                var id = await _marcaService.CrearAsync(RucEmpresa, request);
                return CreatedAtAction(nameof(Obtener), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear marca");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarMarcaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Nombre))
                    return BadRequest(new { mensaje = "El nombre es requerido" });

                var success = await _marcaService.ActualizarAsync(RucEmpresa, id, request);
                if (!success) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar marca");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var success = await _marcaService.EliminarAsync(RucEmpresa, id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar marca");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}