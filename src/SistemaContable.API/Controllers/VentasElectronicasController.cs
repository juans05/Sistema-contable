using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.DTOs.Requests.Venta;
using SistemaContable.Application.DTOs.Responses.Venta;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;
using System.Text.RegularExpressions;
using System.Text;
using System.Xml.Linq;
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
        private string GetCurrentRuc()
        {
            if (!string.IsNullOrEmpty(_RucEmpresa)) return _RucEmpresa;
            return User.FindFirst("RUC")?.Value;
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

                var currentRuc = GetCurrentRuc();
                if (string.IsNullOrEmpty(currentRuc))
                {
                     return BadRequest(new { mensaje = "No se pudo identificar la empresa (RUC no encontrado)" });
                }

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
                var resultado = await _service.ProcesarXmlYRegistrarVentaAsync(archivos, usuario, currentRuc);

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
            [FromQuery] string? fechaDesde = null,
            [FromQuery] string? fechaHasta = null,
            [FromQuery] string? rucCliente = null,
            [FromQuery] string? tipoDoc = null,
            [FromQuery] string? estadoDoc = null,
            [FromQuery] string? filtro = null,
            [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var currentRuc = GetCurrentRuc();
                var usuario = User.Identity?.Name ?? "SYSTEM";
                
                _logger.LogInformation("Listando ventas. RUC: {Ruc}, Fechas: {Desde} - {Hasta}, Page: {Page}", currentRuc, fechaDesde, fechaHasta, page);

                var ventas = await _service.ListarVentasAsync(
                    fechaDesde, fechaHasta, rucCliente, tipoDoc, estadoDoc, currentRuc, filtro, page, pageSize);

                return Ok(new
                {
                    total = ventas.Count, // Nota: esto es el total de la página actual, idealmente el repo debería devolver count total. Por ahora mantenemos contrato.
                    ventas = ventas
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando ventas");
                return StatusCode(500, new { mensaje = "Error al listar ventas" });
            }
        }

        [HttpGet("{id}/xml")]
        public async Task<IActionResult> DescargarXml(int id)
        {
            try
            {
                var xml = await _service.ObtenerXmlVentaAsync(id);
                if (string.IsNullOrEmpty(xml)) return NotFound("XML no encontrado");

                var bytes = Encoding.UTF8.GetBytes(xml);
                return File(bytes, "application/xml", $"Venta_{id}.xml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error descargando XML {Id}", id);
                return StatusCode(500, "Error interno");
            }
        }

        [HttpGet("excel")]
        public async Task<IActionResult> DescargarExcel(
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] string rucCliente = null,
            [FromQuery] string tipoDoc = null,
            [FromQuery] string estadoDoc = null)
        {
            try
            {
                var currentRuc = GetCurrentRuc();
                var archivo = await _service.GenerarReporteExcelVentasAsync(
                   fechaDesde, fechaHasta, rucCliente, tipoDoc, estadoDoc, currentRuc);

                return File(archivo, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Ventas_{DateTime.Now:yyyyMMdd}.xlsx");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando Excel");
                return StatusCode(500, "Error generando reporte");
            }
        }

        [HttpPut("{id}/anular")]
        [ProducesResponseType(typeof(AnularVentaResponseDTO), 200)]
        public async Task<ActionResult<AnularVentaResponseDTO>> AnularVenta(int id, [FromBody] AnularVentaRequest request)
        {
            try
            {
                var usuario = User.Identity?.Name ?? "SYSTEM";
                var resultado = await _service.AnularVentaAsync(id, request.Motivo, usuario);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anulando venta {Id}", id);
                return StatusCode(500, new { mensaje = "Error al anular la venta" });
            }
        }
    }
}
