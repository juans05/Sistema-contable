 using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Requests.Contadores;
using SistemaContable.Application.DTOs.Requests.Empresa;
using SistemaContable.Application.DTOs.Responses;
using SistemaContable.Application.DTOs.Responses.Contador;
using SistemaContable.Application.DTOs.Responses.Empresa;
using SistemaContable.Application.Services.Interfaces;
using System.Security.Claims;

namespace SistemaContable.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class EmpresaController : ControllerBase
    {
        private readonly IEmpresaService _empresaService;
        private readonly ILogger<EmpresaController> _logger;

        public EmpresaController(
            IEmpresaService empresaService,
            ILogger<EmpresaController> logger)
        {
            _empresaService = empresaService;
            _logger = logger;
        }

        /// <summary>
        /// Listar empresas con filtros y paginación
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(PagedEmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedEmpresaResponse>> ListarEmpresas(
            [FromQuery] EmpresaQueryRequest request)
        {
            try
            {
                var response = await _empresaService.ListarEmpresasAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar empresas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener empresa por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(EmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmpresaResponse>> ObtenerEmpresa(Guid id)
        {
            try
            {
                var response = await _empresaService.ObtenerEmpresaPorIdAsync(id);

                if (response == null)
                {
                    return NotFound(new { message = "Empresa no encontrada" });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresa {EmpresaId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Crear nueva empresa
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CreateEmpresaResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CreateEmpresaResponse>> CrearEmpresa(
            [FromBody] CreateEmpresaRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _empresaService.CrearEmpresaAsync(request);
                    
                if (!response.Success)
                {
                    return BadRequest(new
                    {
                        message = response.Message,
                        errors = response.Success
                    });
                }

                return CreatedAtAction(
                    nameof(ObtenerEmpresa),
                    new { id = response },
                    response
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear empresa");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Actualizar empresa existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(UpdateEmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UpdateEmpresaResponse>> ActualizarEmpresa(
            Guid id,
            [FromBody] UpdateEmpresaRequest request)
        {
            try
            {
                if (id != request.Id)
                {
                    return BadRequest(new { message = "El ID de la URL no coincide con el ID del body" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _empresaService.ActualizarEmpresaAsync(request);

                if (!response.Success)
                {
                    if (response.Message.Contains("no encontrada"))
                    {
                        return NotFound(new
                        {
                            message = response.Message,
                            errors = response.Success
                        });
                    }
                    return BadRequest(new
                    {
                        message = response.Message,
                        errors = response.Success
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar empresa {EmpresaId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Eliminar empresa (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(DeleteEmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DeleteEmpresaResponse>> EliminarEmpresa(Guid id)
        {
            try
            {
                var response = await _empresaService.EliminarEmpresaAsync(id);

                if (!response.Success)
                {
                    return NotFound(new
                    {
                        message = response.Message
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar empresa {EmpresaId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Asignar contador a empresa
        /// </summary>
        [HttpPost("asignar-contador")]
        [ProducesResponseType(typeof(AsignarContadorResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AsignarContadorResponse>> AsignarContador(
            [FromBody] AsignarContadorRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Obtener el ID del usuario actual desde el token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int asignadoPor))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var response = await _empresaService.AsignarContadorAsync(request, asignadoPor);

                if (!response.Success)
                {
                    return BadRequest(new
                    {
                        message = response.Message,
                        errors = response.Success
                    });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al asignar contador");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Listar contadores disponibles
        /// </summary>
        [HttpGet("contadores")]
        [ProducesResponseType(typeof(ContadoresResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ContadoresResponse>> ListarContadores()
        {
            try
            {
                var response = await _empresaService.ListarContadoresAsync();
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al listar contadores");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener mis empresas (como contador)
        /// </summary>
        [HttpGet("mis-empresas")]
        [ProducesResponseType(typeof(PagedEmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedEmpresaResponse>> ObtenerMisEmpresas(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int usuarioId))
                {
                    return Unauthorized(new { message = "Usuario no autenticado" });
                }

                var request = new EmpresaQueryRequest
                {
                    UsuarioId = usuarioId,
                    Activo = true,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

                var response = await _empresaService.ListarEmpresasAsync(request);

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener mis empresas");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cambiar estado de empresa (Activar/Desactivar)
        /// </summary>
        [HttpPatch("{id}/estado")]
        [ProducesResponseType(typeof(CambiarEstadoResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CambiarEstadoResponse>> CambiarEstadoEmpresa(Guid id,[FromBody] CambiarEstadoRequest request)
        {
            try
            {
                if (id != request.EmpresaId)
                {
                    return BadRequest(new { message = "El ID de la URL no coincide con el ID del body" });
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var response = await _empresaService.CambiarEstadoEmpresaAsync(request);

                if (!response.Success)
                {
                    if (response.Message.Contains("no encontrada"))
                    {
                        return NotFound(new { message = response.Message });
                    }
                    return BadRequest(new { message = response.Message });
                }

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cambiar estado de empresa {EmpresaId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener empresas por contador específico
        /// </summary>
        [HttpGet("contador/{contadorId}")]
        [ProducesResponseType(typeof(PagedEmpresaResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedEmpresaResponse>> ObtenerEmpresasPorContador(
            int contadorId, string? search,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            try
            {
                var request = new EmpresaQueryRequest
                {
                    UsuarioId = contadorId,
                   
                    Activo = true,
                    PageNumber = pageNumber,
                    Search = search,
                    PageSize = pageSize
                };

                var response = await _empresaService.ListarEmpresasAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener empresas del contador {ContadorId}", contadorId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
