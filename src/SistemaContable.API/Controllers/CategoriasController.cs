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
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaService _categoriaService;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly ILogger<CategoriasController> _logger;

        public CategoriasController(
            ICategoriaService categoriaService, 
            IRucEmpresaService rucEmpresaService, 
            ILogger<CategoriasController> logger)
        {
            _categoriaService = categoriaService;
            _rucEmpresaService = rucEmpresaService;
            _logger = logger;
        }

        private string RucEmpresa => _rucEmpresaService.ObtenerRucActual() 
            ?? throw new UnauthorizedAccessException("RUC de empresa no encontrado en el token");

        [HttpGet]
        [ProducesResponseType(typeof(List<CategoriaDto>), 200)]
        public async Task<IActionResult> Listar([FromQuery] bool? activo = null)
        {
            try
            {
                var result = await _categoriaService.ListarAsync(RucEmpresa, activo);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar categorías");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoriaDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Obtener(int id)
        {
            try
            {
                var result = await _categoriaService.ObtenerPorIdAsync(RucEmpresa, id);
                if (result == null) return NotFound();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPost]
        [ProducesResponseType(typeof(int), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Crear([FromBody] CrearCategoriaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Nombre))
                    return BadRequest(new { mensaje = "El nombre es requerido" });

                var id = await _categoriaService.CrearAsync(RucEmpresa, request);
                return CreatedAtAction(nameof(Obtener), new { id }, new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarCategoriaRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Nombre))
                    return BadRequest(new { mensaje = "El nombre es requerido" });

                var success = await _categoriaService.ActualizarAsync(RucEmpresa, id, request);
                if (!success) return NotFound();

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría");
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
                var success = await _categoriaService.EliminarAsync(RucEmpresa, id);
                if (!success) return NotFound();
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría");
                return StatusCode(500, new { mensaje = "Error interno del servidor" });
            }
        }
    }
}