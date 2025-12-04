using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;
using System.Security.Claims;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VentasElectronicasController : ControllerBase
    {
        private readonly IVentaElectronicaService _service;
        private readonly ILogger<VentasElectronicasController> _logger;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly string _RucEmpresa = "";
        public VentasElectronicasController(
            IVentaElectronicaService service, IRucEmpresaService rucEmpresaService,
            ILogger<VentasElectronicasController> logger)
        {
            _rucEmpresaService = rucEmpresaService;
            _service = service;
            _logger = logger;
            _RucEmpresa = _rucEmpresaService.ObtenerRucActual();
        }
        [HttpPost("procesar-xml")]
        [ProducesResponseType(typeof(ProcesarXmlResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcesarXmlResponseDto>> ProcesarXmlVentas(
            [FromForm] List<IFormFile> archivos)
        {
            try
            {
                // ✅ Obtener RUC automáticamente
              //  var rucEmpresa = _rucEmpresaService.ObtenerRucActual();
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    _logger.LogWarning("Token válido pero sin UserId en claims");
                    return Unauthorized(new { message = "Usuario no identificado en el token" });
                }

                var userId = int.Parse(userIdClaim);

                // Extraer EmpresaId del token o del query parameter
                var empresaIdClaim = User.FindFirst("RUC")?.Value;
                Guid currentEmpresaId;
                if (archivos == null || !archivos.Any())
                    return BadRequest(new { mensaje = "Debe enviar al menos un archivo XML" });

                // Validar extensiones
                var archivosInvalidos = archivos
                    .Where(a => !a.FileName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                    .ToList();

                if (archivosInvalidos.Any())
                    return BadRequest(new
                    {
                        mensaje = "Solo se permiten archivos XML",
                        archivosInvalidos = archivosInvalidos.Select(a => a.FileName)
                    });

                // Validar tamaño (5MB por archivo)
                var archivosGrandes = archivos.Where(a => a.Length > 5 * 1024 * 1024).ToList();
                if (archivosGrandes.Any())
                    return BadRequest(new
                    {
                        mensaje = "Archivos muy grandes (máx 5MB)",
                        archivos = archivosGrandes.Select(a => a.FileName)
                    });

                var usuario = User.Identity?.Name ?? "SYSTEM";
                var resultado = await _service.ProcesarXmlYRegistrarVentaAsync(archivos, usuario, (_RucEmpresa == null) ? empresaIdClaim : _RucEmpresa);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando archivos XML");
                return StatusCode(500, new
                {
                    mensaje = "Error al procesar archivos XML",
                    detalle = ex.Message
                });
            }
        }
        /// <summary>
        /// Obtiene una venta completa con sus detalles
        /// </summary>
        /// <param name="id">ID de la venta</param>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(VentaCompletaDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<VentaCompletaDto>> ObtenerVenta(int id)
        {
            try
            {
                var venta = await _service.ObtenerVentaPorIdAsync(id);

                if (venta == null)
                    return NotFound(new { mensaje = $"Venta {id} no encontrada" });

                return Ok(venta);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo venta {Id}", id);
                return StatusCode(500, new { mensaje = "Error al obtener la venta" });
            }
        }

        /// <summary>
        /// Lista ventas con filtros
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<VentaListaDto>), 200)]
        public async Task<ActionResult<List<VentaListaDto>>> ListarVentas(
            [FromQuery] DateTime fechaDesde,
            [FromQuery] DateTime fechaHasta,
            [FromQuery] string rucCliente = null,
            [FromQuery] string tipoDoc = null,
            [FromQuery] string estadoDoc = null)
        {
            try
            {

                var usuario = User.Identity?.Name ?? "SYSTEM";
                var ventas = await _service.ListarVentasAsync(
                    fechaDesde, fechaHasta, rucCliente, tipoDoc, estadoDoc, _RucEmpresa);

                return Ok(new
                {
                    total = ventas.Count,
                    ventas = ventas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando ventas");
                return StatusCode(500, new { mensaje = "Error al listar ventas" });
            }
        }
    }
}
