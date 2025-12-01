using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VentasElectronicasController : ControllerBase
    {
        private readonly IVentaElectronicaService _service;
        private readonly ILogger<VentasElectronicasController> _logger;

        public VentasElectronicasController(
            IVentaElectronicaService service,
            ILogger<VentasElectronicasController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpPost("procesar-xml")]
        [ProducesResponseType(typeof(ProcesarXmlResponseDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcesarXmlResponseDto>> ProcesarXmlVentas(
            [FromForm] List<IFormFile> archivos)
        {
            try
            {
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
                var resultado = await _service.ProcesarXmlYRegistrarVentaAsync(archivos, usuario);

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
                var ventas = await _service.ListarVentasAsync(
                    fechaDesde, fechaHasta, rucCliente, tipoDoc, estadoDoc);

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
