using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.DTOs.Requests.Compra;
using SistemaContable.Application.DTOs.Responses.Compra;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompraController : ControllerBase
    {
        private readonly ICompraService _compraService;
        private readonly ILogger<CompraController> _logger;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly string _RucEmpresa = "";

        public CompraController(
            ICompraService compraService, IRucEmpresaService rucEmpresaService,
            ILogger<CompraController> logger)
        {
            _rucEmpresaService = rucEmpresaService;
            _compraService = compraService;
            _logger = logger;
            _RucEmpresa = _rucEmpresaService.ObtenerRucActual();
        }

        [HttpPost("procesar-xml")]
        [ProducesResponseType(typeof(ProcesarXmlCompraRespondeDto), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<ProcesarXmlCompraRespondeDto>> ProcesarXmlCompras(
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
                var resultado = await _compraService.ProcesarXmlYRegistrarCompraAsync(archivos, usuario, (_RucEmpresa == null) ? empresaIdClaim : _RucEmpresa);

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
        /// Lista compras con filtros
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<CompraListaDto>), 200)]
        public async Task<ActionResult<List<CompraListaDto>>> ListarCompras(
            [FromQuery] string fechaDesde,
            [FromQuery] string fechaHasta,
            [FromQuery] string rucProveedor = null,
            [FromQuery] string tipoDoc = null,
            [FromQuery] string estadoDoc = null) 
        {
            var compras = await _compraService.ListarComprasAsync(
                fechaDesde, fechaHasta,
                rucProveedor, tipoDoc,
                estadoDoc, _RucEmpresa);

            return Ok(new
            {
                total = compras.Count,
                compras = compras
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CompraCompletaDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<CompraCompletaDto>> ObtenerCompra(int id) 
        {
            try
            {
                var compra = await _compraService.ObtenerCompraPorIdAsync(id);

                if (compra == null)
                    return NotFound(new { mensaje = $"Compra {id} no encontrada" });

                return Ok(compra);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo venta {Id}", id);
                return StatusCode(500, new { mensaje = "Error al obtener la venta" });
            }
        }

        [HttpPut("{id}/anular")]
        [ProducesResponseType(typeof(AnularCompraResponseDTO), 200)]
        public async Task<ActionResult<AnularCompraResponseDTO>> AnularCompra(int id, [FromBody] AnularCompraRequest request) 
        {
            try
            {
                var usuario = User.Identity?.Name ?? "SYSTEM";
                var resultado = await _compraService.AnularCompraAsync(id, request.Motivo, usuario);

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
