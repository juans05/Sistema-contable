using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Models;
using System.Security.Claims;

namespace SistemaContable.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductoController : ControllerBase
    {
        private readonly IProductoService _productoService;
        private readonly IRucEmpresaService _rucEmpresaService;
        private readonly ILogger<ProductoController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly string? _RucEmpresa;

        public ProductoController(
            IProductoService productoService,
            IRucEmpresaService rucEmpresaService,
            ILogger<ProductoController> logger,
            IWebHostEnvironment environment)
        {
            _productoService = productoService;
            _rucEmpresaService = rucEmpresaService;
            _logger = logger;
            _environment = environment;
            _RucEmpresa = _rucEmpresaService.ObtenerRucActual();
        }

        // ... Metodos existentes ...

        /// <summary>
        /// Lista todas las monedas activas
        /// </summary>
        [HttpGet("monedas")]
        [ProducesResponseType(typeof(List<MonedaDto>), 200)]
        public async Task<ActionResult<List<MonedaDto>>> ListarMonedas()
        {
            try
            {
                var monedas = await _productoService.ListarMonedasAsync();
                return Ok(monedas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando monedas");
                return StatusCode(500, new { mensaje = "Error al listar monedas" });
            }
        }

        /// <summary>
        /// Lista el plan contable activo
        /// </summary>
        [HttpGet("cuentas-contables")]
        [ProducesResponseType(typeof(List<PlanContableDto>), 200)]
        public async Task<ActionResult<List<PlanContableDto>>> ListarCuentasContables()
        {
            try
            {
                var cuentas = await _productoService.ListarCuentasContablesAsync();
                return Ok(cuentas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando cuentas contables");
                return StatusCode(500, new { mensaje = "Error al listar cuentas contables" });
            }
        }

        /// <summary>
        /// Sube una imagen de producto
        /// </summary>
        [HttpPost("upload-image")]
        [ProducesResponseType(typeof(object), 200)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> SubirImagen(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { mensaje = "No se ha proporcionado ningún archivo" });

                // Validar extensión
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var permitidas = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                if (!permitidas.Contains(extension))
                    return BadRequest(new { mensaje = "Formato de imagen no permitido" });

                // Crear directorio si no existe
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "productos");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                // Nombre único
                var fileName = $"{Guid.NewGuid()}{extension}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // URL relativa para guardar en BD
                var fileUrl = $"/uploads/productos/{fileName}";

                return Ok(new { url = fileUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error subiendo imagen");
                return StatusCode(500, new { mensaje = "Error al subir la imagen" });
            }
        }

        private string GetCurrentRuc()
        {
            if (!string.IsNullOrEmpty(_RucEmpresa)) return _RucEmpresa;
            return User.FindFirst("RUC")?.Value ?? "";
        }

        private string GetCurrentUser()
        {
            return User.Identity?.Name ?? "SYSTEM";
        }

        /// <summary>
        /// Lista todos los productos con paginación y filtros
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(ListarProductosResponse), 200)]
        public async Task<ActionResult<ListarProductosResponse>> Listar(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filtro = null)
        {
            try
            {
                var currentRuc = GetCurrentRuc();
                if (string.IsNullOrEmpty(currentRuc))
                {
                    return BadRequest(new { mensaje = "No se pudo identificar la empresa (RUC no encontrado)" });
                }

                var (productos, total) = await _productoService.ListarProductosAsync(currentRuc, page, pageSize, filtro);

                return Ok(new ListarProductosResponse
                {
                    Productos = productos,
                    Total = total
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando productos");
                return StatusCode(500, new { mensaje = "Error al listar productos" });
            }
        }

        /// <summary>
        /// Obtiene un producto por ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ProductoDetalleDto), 200)]
        [ProducesResponseType(404)]
        public async Task<ActionResult<ProductoDetalleDto>> ObtenerPorId(int id)
        {
            try
            {
                var producto = await _productoService.ObtenerProductoPorIdAsync(id);

                if (producto == null)
                    return NotFound(new { mensaje = $"Producto {id} no encontrado" });

                return Ok(producto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo producto {Id}", id);
                return StatusCode(500, new { mensaje = "Error al obtener el producto" });
            }
        }

        /// <summary>
        /// Crea un nuevo producto
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(CrearProductoResponse), 200)]
        [ProducesResponseType(400)]
        public async Task<ActionResult<CrearProductoResponse>> Crear([FromBody] CrearProductoRequest request)
        {
            try
            {
                var currentRuc = GetCurrentRuc();
                if (string.IsNullOrEmpty(currentRuc))
                {
                    return BadRequest(new { mensaje = "No se pudo identificar la empresa (RUC no encontrado)" });
                }

                var currentUser = GetCurrentUser();
                var idProducto = await _productoService.CrearProductoAsync(request, currentUser, currentRuc);

                return Ok(new CrearProductoResponse
                {
                    IdProducto = idProducto,
                    Mensaje = "Producto creado exitosamente"
                });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al crear producto");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumentos inválidos al crear producto");
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando producto");
                return StatusCode(500, new { mensaje = "Error al crear el producto" });
            }
        }

        /// <summary>
        /// Actualiza un producto existente
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(400)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Actualizar(int id, [FromBody] ActualizarProductoRequest request)
        {
            try
            {
                var currentRuc = GetCurrentRuc();
                if (string.IsNullOrEmpty(currentRuc))
                {
                    return BadRequest(new { mensaje = "No se pudo identificar la empresa (RUC no encontrado)" });
                }

                var currentUser = GetCurrentUser();
                var success = await _productoService.ActualizarProductoAsync(id, request, currentUser, currentRuc);

                if (!success)
                    return NotFound(new { mensaje = $"Producto {id} no encontrado" });

                return Ok(new { mensaje = "Producto actualizado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al actualizar producto {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Argumentos inválidos al actualizar producto {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando producto {Id}", id);
                return StatusCode(500, new { mensaje = "Error al actualizar el producto" });
            }
        }

        /// <summary>
        /// Elimina (desactiva) un producto
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                var success = await _productoService.EliminarProductoAsync(id);

                if (!success)
                    return NotFound(new { mensaje = $"Producto {id} no encontrado" });

                return Ok(new { mensaje = "Producto eliminado exitosamente" });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Validación fallida al eliminar producto {Id}", id);
                return BadRequest(new { mensaje = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando producto {Id}", id);
                return StatusCode(500, new { mensaje = "Error al eliminar el producto" });
            }
        }

        /// <summary>
        /// Lista todas las categorías activas
        /// </summary>
        [HttpGet("categorias")]
        [ProducesResponseType(typeof(List<CategoriaDto>), 200)]
        public async Task<ActionResult<List<CategoriaDto>>> ListarCategorias()
        {
            try
            {
                var categorias = await _productoService.ListarCategoriasAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando categorías");
                return StatusCode(500, new { mensaje = "Error al listar categorías" });
            }
        }

        /// <summary>
        /// Lista todas las marcas activas
        /// </summary>
        [HttpGet("marcas")]
        [ProducesResponseType(typeof(List<MarcaDto>), 200)]
        public async Task<ActionResult<List<MarcaDto>>> ListarMarcas()
        {
            try
            {
                var marcas = await _productoService.ListarMarcasAsync();
                return Ok(marcas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando marcas");
                return StatusCode(500, new { mensaje = "Error al listar marcas" });
            }
        }

        /// <summary>
        /// Lista todas las unidades de medida activas
        /// </summary>
        [HttpGet("unidades-medida")]
        [ProducesResponseType(typeof(List<UnidadMedidaDto>), 200)]
        public async Task<ActionResult<List<UnidadMedidaDto>>> ListarUnidadesMedida()
        {
            try
            {
                var unidades = await _productoService.ListarUnidadesMedidaAsync();
                return Ok(unidades);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando unidades de medida");
                return StatusCode(500, new { mensaje = "Error al listar unidades de medida" });
            }
        }
    }

    // Response DTOs adicionales
    public class CrearProductoResponse
    {
        public int IdProducto { get; set; }
        public string Mensaje { get; set; }
    }
}
