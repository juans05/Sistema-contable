using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class ProductoRepository : IProductoRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<ProductoRepository> _logger;

        public ProductoRepository(
            IConfiguration configuration,
            ILogger<ProductoRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no configurada");
            _logger = logger;
        }

        public async Task<(List<ProductoDto> productos, int total)> ListarAsync(
            string rucEmpresa, int page, int pageSize, string? filtro = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Query con joins a tablas relacionadas
                var query = @"
                    SELECT 
                        p.id,
                        p.codigo,
                        p.codigo_barras AS codigoBarras,
                        p.nombre,
                        p.descripcion,
                        COALESCE(c.nombre, '') AS categoria,
                        COALESCE(m.nombre, '') AS marca,
                        p.precio_venta AS precioVenta,
                        p.precio_compra AS precioCompra,
                        p.margen_utilidad AS margenUtilidad,
                        COALESCE(u.codigo, '') AS unidadMedida,
                        p.stock_actual AS stockActual,
                        p.stock_minimo AS stockMinimo,
                        p.stock_maximo AS stockMaximo,
                        p.afecto_igv AS afectoIgv,
                        p.activo,
                        COUNT(*) OVER() AS total
                    FROM productos p
                    LEFT JOIN categorias c ON p.categoria_id = c.id
                    LEFT JOIN marcas m ON p.marca_id = m.id
                    LEFT JOIN unidades_medida u ON p.unidad_medida_id = u.id
                    WHERE p.empresa_id = (SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1)
                        AND (@filtro IS NULL OR 
                             p.nombre ILIKE '%' || @filtro || '%' OR 
                             p.codigo ILIKE '%' || @filtro || '%' OR
                             p.codigo_barras ILIKE '%' || @filtro || '%')
                    ORDER BY p.created_at DESC
                    LIMIT @pageSize OFFSET @offset";

                var results = await connection.QueryAsync<ProductoDtoDb>(
                    query,
                    new
                    {
                        rucEmpresa,
                        filtro,
                        pageSize,
                        offset = (page - 1) * pageSize
                    },
                    commandTimeout: 30
                );

                var productos = results.Select(r => new ProductoDto
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    CodigoBarras = r.codigobarras,
                    Nombre = r.nombre,
                    Descripcion = r.descripcion,
                    Categoria = r.categoria,
                    Marca = r.marca,
                    PrecioVenta = r.precioventa,
                    PrecioCompra = r.preciocompra,
                    MargenUtilidad = r.margenutilidad,
                    UnidadMedida = r.unidadmedida,
                    StockActual = r.stockactual,
                    StockMinimo = r.stockminimo,
                    StockMaximo = r.stockmaximo,
                    AfectoIgv = r.afectoigv,
                    Activo = r.activo
                }).ToList();

                var total = results.FirstOrDefault()?.total ?? 0;
                return (productos, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando productos");
                throw;
            }
        }

        public async Task<ProductoDetalleDto?> ObtenerPorIdAsync(int idProducto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"
                    SELECT 
                        p.id, p.empresa_id AS empresaid,
                        p.codigo, p.codigo_barras AS codigobarras, p.nombre, p.descripcion,
                        p.categoria_id AS categoriaid, COALESCE(c.nombre, '') AS categoria,
                        p.marca_id AS marcaid, COALESCE(m.nombre, '') AS marca,
                        p.linea_id AS lineaid, COALESCE(l.nombre, '') AS linea,
                        p.afecto_igv AS afectoigv,
                        p.cuenta_venta_id AS cuentaventaid, pcv.codigo AS cuentaventacodigo,
                        p.cuenta_costo_id AS cuentacostoid, pcc.codigo AS cuentacostocodigo,
                        p.codigo_sunat AS codigosunat,
                        p.requiere_inspeccion AS requiereinspeccion,
                        p.precio_compra AS preciocompra,
                        p.margen_utilidad AS margenutilidad,
                        p.precio_venta AS precioventa,
                        p.moneda_id AS monedaid, mon.codigo AS moneda,
                        p.unidad_medida_id AS unidadmedidaid, COALESCE(u.codigo, '') AS unidadmedida,
                        p.stock_minimo AS stockminimo, p.stock_maximo AS stockmaximo, p.stock_actual AS stockactual,
                        p.ubicacion_fisica AS ubicacionfisica,
                        p.margen_inventario AS margeninventario,
                        p.margen_lotes AS margenlotes,
                        p.margen_series AS margenseries,
                        p.margen_vencimiento AS margenvencimiento,
                        p.imagen_url AS imagenurl,
                        p.activo,
                        p.sujeto_retencion AS sujetoretencion,
                        p.cuenta_compra_id AS cuentacompraid, pccomp.codigo AS cuentacompracodigo,
                        p.cuenta_inventario_id AS cuentainventarioid, pcinv.codigo AS cuentainventariocodigo
                    FROM productos p
                    LEFT JOIN categorias c ON p.categoria_id = c.id
                    LEFT JOIN marcas m ON p.marca_id = m.id
                    LEFT JOIN lineas_producto l ON p.linea_id = l.id
                    LEFT JOIN unidades_medida u ON p.unidad_medida_id = u.id
                    LEFT JOIN monedas mon ON p.moneda_id = mon.id
                    LEFT JOIN contabilidad_plan_cuentas pcv ON p.cuenta_venta_id = pcv.id
                    LEFT JOIN contabilidad_plan_cuentas pcc ON p.cuenta_costo_id = pcc.id
                    LEFT JOIN contabilidad_plan_cuentas pccomp ON p.cuenta_compra_id = pccomp.id
                    LEFT JOIN contabilidad_plan_cuentas pcinv ON p.cuenta_inventario_id = pcinv.id
                    WHERE p.id = @idProducto";

                var result = await connection.QueryFirstOrDefaultAsync<ProductoDetalleDtoDb>(
                    query,
                    new { idProducto },
                    commandTimeout: 30
                );

                if (result == null) return null;

                return new ProductoDetalleDto
                {
                    Id = result.id,
                    EmpresaId = result.empresaid,
                    Codigo = result.codigo,
                    CodigoBarras = result.codigobarras,
                    Nombre = result.nombre,
                    Descripcion = result.descripcion,
                    CategoriaId = result.categoriaid,
                    Categoria = result.categoria,
                    MarcaId = result.marcaid,
                    Marca = result.marca,
                    LineaId = result.lineaid,
                    Linea = result.linea,
                    AfectoIgv = result.afectoigv,
                    CuentaVentaId = result.cuentaventaid,
                    CuentaVentaCodigo = result.cuentaventacodigo,
                    CuentaCostoId = result.cuentacostoid,
                    CuentaCostoCodigo = result.cuentacostocodigo,
                    CodigoSunat = result.codigosunat,
                    RequiereInspeccion = result.requiereinspeccion,
                    PrecioCompra = result.preciocompra,
                    MargenUtilidad = result.margenutilidad,
                    PrecioVenta = result.precioventa,
                    MonedaId = result.monedaid,
                    Moneda = result.moneda,
                    UnidadMedidaId = result.unidadmedidaid,
                    UnidadMedida = result.unidadmedida,
                    StockMinimo = result.stockminimo,
                    StockMaximo = result.stockmaximo,
                    StockActual = result.stockactual,
                    UbicacionFisica = result.ubicacionfisica,
                    MargenInventario = result.margeninventario,
                    MargenLotes = result.margenlotes,
                    MargenSeries = result.margenseries,
                    MargenVencimiento = result.margenvencimiento,
                    ImagenUrl = result.imagenurl,
                    Activo = result.activo,
                    SujetoRetencion = result.sujetoretencion,
                    CuentaCompraId = result.cuentacompraid,
                    CuentaInventarioId = result.cuentainventarioid
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo producto por ID {IdProducto}", idProducto);
                return null;
            }
        }

        public async Task<int> CrearAsync(string rucEmpresa, CrearProductoRequest request, string usuario)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Calcular precio de venta si hay margen
                var precioVenta = request.MargenUtilidad.HasValue
                    ? request.PrecioCompra * (1 + request.MargenUtilidad.Value / 100)
                    : request.PrecioCompra;

                var query = @"
                    INSERT INTO productos (
                        empresa_id, codigo, codigo_barras, nombre, descripcion,
                        categoria_id, marca_id, linea_id,
                        afecto_igv, cuenta_venta_id, cuenta_costo_id, codigo_sunat, requiere_inspeccion,
                        precio_compra, margen_utilidad, precio_venta, moneda_id,
                        unidad_medida_id, stock_minimo, stock_maximo, stock_actual, ubicacion_fisica,
                        margen_inventario, margen_lotes, margen_series, margen_vencimiento,
                        imagen_url, activo, created_at,
                        sujeto_retencion, cuenta_compra_id, cuenta_inventario_id
                    ) VALUES (
                        (SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1),
                        @codigo, @codigoBarras, @nombre, @descripcion,
                        @categoriaId, @marcaId, @lineaId,
                        @afectoIgv, @cuentaVentaId, @cuentaCostoId, @codigoSunat, @requiereInspeccion,
                        @precioCompra, @margenUtilidad, @precioVenta, @monedaId,
                        @unidadMedidaId, @stockMinimo, @stockMaximo, 0, @ubicacionFisica,
                        @margenInventario, @margenLotes, @margenSeries, @margenVencimiento,
                        @imagenUrl, TRUE, NOW(),
                        @sujetoRetencion, @cuentaCompraId, @cuentaInventarioId
                    ) RETURNING id";

                var idProducto = await connection.ExecuteScalarAsync<int>(
                    query,
                    new
                    {
                        rucEmpresa,
                        codigo = request.Codigo,
                        codigoBarras = request.CodigoBarras,
                        nombre = request.Nombre,
                        descripcion = request.Descripcion,
                        categoriaId = request.CategoriaId,
                        marcaId = request.MarcaId,
                        lineaId = request.LineaId,
                        afectoIgv = request.AfectoIgv,
                        cuentaVentaId = request.CuentaVentaId,
                        cuentaCostoId = request.CuentaCostoId,
                        codigoSunat = request.CodigoSunat,
                        requiereInspeccion = request.RequiereInspeccion,
                        precioCompra = request.PrecioCompra,
                        margenUtilidad = request.MargenUtilidad,
                        precioVenta,
                        monedaId = request.MonedaId,
                        unidadMedidaId = request.UnidadMedidaId,
                        stockMinimo = request.StockMinimo,
                        stockMaximo = request.StockMaximo,
                        ubicacionFisica = request.UbicacionFisica,
                        margenInventario = request.MargenInventario,
                        margenLotes = request.MargenLotes,
                        margenSeries = request.MargenSeries,
                        margenVencimiento = request.MargenVencimiento,
                        imagenUrl = request.ImagenUrl,
                        sujetoRetencion = request.SujetoRetencion,
                        cuentaCompraId = request.CuentaCompraId,
                        cuentaInventarioId = request.CuentaInventarioId
                    },
                    commandTimeout: 30
                );

                _logger.LogInformation("Producto creado con ID: {IdProducto}", idProducto);
                return idProducto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando producto");
                throw;
            }
        }

        public async Task<bool> ActualizarAsync(int idProducto, ActualizarProductoRequest request, string usuario)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // Calcular precio de venta si hay margen
                var precioVenta = request.MargenUtilidad.HasValue
                    ? request.PrecioCompra * (1 + request.MargenUtilidad.Value / 100)
                    : request.PrecioCompra;

                var query = @"
                    UPDATE productos SET
                        codigo = @codigo, codigo_barras = @codigoBarras, nombre = @nombre, descripcion = @descripcion,
                        categoria_id = @categoriaId, marca_id = @marcaId, linea_id = @lineaId,
                        afecto_igv = @afectoIgv, cuenta_venta_id = @cuentaVentaId, cuenta_costo_id = @cuentaCostoId, 
                        codigo_sunat = @codigoSunat, requiere_inspeccion = @requiereInspeccion,
                        precio_compra = @precioCompra, margen_utilidad = @margenUtilidad, precio_venta = @precioVenta, moneda_id = @monedaId,
                        unidad_medida_id = @unidadMedidaId, stock_minimo = @stockMinimo, stock_maximo = @stockMaximo, 
                        ubicacion_fisica = @ubicacionFisica,
                        margen_inventario = @margenInventario, margen_lotes = @margenLotes, margen_series = @margenSeries, 
                        margen_vencimiento = @margenVencimiento,
                        imagen_url = @imagenUrl, activo = @activo, updated_at = NOW(),
                        sujeto_retencion = @sujetoRetencion, cuenta_compra_id = @cuentaCompraId, cuenta_inventario_id = @cuentaInventarioId
                    WHERE id = @idProducto";

                var rowsAffected = await connection.ExecuteAsync(
                    query,
                    new
                    {
                        idProducto,
                        codigo = request.Codigo,
                        codigoBarras = request.CodigoBarras,
                        nombre = request.Nombre,
                        descripcion = request.Descripcion,
                        categoriaId = request.CategoriaId,
                        marcaId = request.MarcaId,
                        lineaId = request.LineaId,
                        afectoIgv = request.AfectoIgv,
                        cuentaVentaId = request.CuentaVentaId,
                        cuentaCostoId = request.CuentaCostoId,
                        codigoSunat = request.CodigoSunat,
                        requiereInspeccion = request.RequiereInspeccion,
                        precioCompra = request.PrecioCompra,
                        margenUtilidad = request.MargenUtilidad,
                        precioVenta,
                        monedaId = request.MonedaId,
                        unidadMedidaId = request.UnidadMedidaId,
                        stockMinimo = request.StockMinimo,
                        stockMaximo = request.StockMaximo,
                        ubicacionFisica = request.UbicacionFisica,
                        margenInventario = request.MargenInventario,
                        margenLotes = request.MargenLotes,
                        margenSeries = request.MargenSeries,
                        margenVencimiento = request.MargenVencimiento,
                        imagenUrl = request.ImagenUrl,
                        activo = request.Activo,
                        sujetoRetencion = request.SujetoRetencion,
                        cuentaCompraId = request.CuentaCompraId,
                        cuentaInventarioId = request.CuentaInventarioId
                    },
                    commandTimeout: 30
                );

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando producto {IdProducto}", idProducto);
                throw;
            }
        }

        public async Task<bool> EliminarAsync(int idProducto)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = "UPDATE productos SET activo = FALSE, updated_at = NOW() WHERE id = @idProducto";
                var rowsAffected = await connection.ExecuteAsync(query, new { idProducto }, commandTimeout: 30);

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando producto {IdProducto}", idProducto);
                throw;
            }
        }

        public async Task<List<CategoriaDto>> ListarCategoriasAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<CategoriaDtoDb>(
                    "SELECT id, nombre, descripcion FROM categorias WHERE activo = TRUE ORDER BY nombre",
                    commandTimeout: 30
                );

                return results.Select(r => new CategoriaDto
                {
                    Id = r.id,
                    Nombre = r.nombre,
                    Descripcion = r.descripcion
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando categorías");
                throw;
            }
        }

        public async Task<List<MarcaDto>> ListarMarcasAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<MarcaDtoDb>(
                    "SELECT id, nombre FROM marcas WHERE activo = TRUE ORDER BY nombre",
                    commandTimeout: 30
                );

                return results.Select(r => new MarcaDto
                {
                    Id = r.id,
                    Nombre = r.nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando marcas");
                throw;
            }
        }

        public async Task<List<UnidadMedidaDto>> ListarUnidadesMedidaAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var results = await connection.QueryAsync<UnidadMedidaDtoDb>(
                    "SELECT id, codigo, nombre FROM unidades_medida WHERE activo = TRUE ORDER BY nombre",
                    commandTimeout: 30
                );

                return results.Select(r => new UnidadMedidaDto
                {
                    Id = r.id,
                    Codigo = r.codigo,
                    Nombre = r.nombre
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando unidades de medida");
                throw;
            }
        }

        public async Task<List<MonedaDto>> ListarMonedasAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var results = await connection.QueryAsync<MonedaDtoDb>("SELECT id, codigo, nombre, simbolo FROM monedas WHERE activo = TRUE ORDER BY id", commandTimeout: 30);
                return results.Select(r => new MonedaDto { Id = r.id, Codigo = r.codigo, Nombre = r.nombre, Simbolo = r.simbolo }).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error monedas"); throw; }
        }

        public async Task<List<PlanContableDto>> ListarCuentasContablesAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                var results = await connection.QueryAsync<PlanContableDtoDb>("SELECT id, codigo, nombre, tipo_cuenta as tipocuenta FROM contabilidad_plan_cuentas WHERE activo = TRUE AND permite_movimiento = TRUE ORDER BY codigo", commandTimeout: 30);
                return results.Select(r => new PlanContableDto { Id = r.id, Codigo = r.codigo, Nombre = r.nombre, TipoCuenta = r.tipocuenta }).ToList();
            }
            catch (Exception ex) { _logger.LogError(ex, "Error cuentas"); throw; }
        }

        public async Task<bool> ExisteSkuAsync(string rucEmpresa, string codigo, int? idProductoExcluir = null)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"SELECT COUNT(*) FROM productos 
                             WHERE empresa_id = (SELECT id FROM empresas WHERE ruc = @rucEmpresa LIMIT 1)
                             AND codigo = @codigo
                             AND (@idProductoExcluir IS NULL OR id != @idProductoExcluir)";

                var count = await connection.ExecuteScalarAsync<int>(
                    query,
                    new { rucEmpresa, codigo, idProductoExcluir },
                    commandTimeout: 10
                );

                return count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia de SKU");
                return false;
            }
        }

        // DTOs internos para mapeo desde la BD (snake_case)
        private class ProductoDtoDb
        {
            public int id { get; set; }
            public string codigo { get; set; }
            public string? codigobarras { get; set; }
            public string nombre { get; set; }
            public string? descripcion { get; set; }
            public string? categoria { get; set; }
            public string? marca { get; set; }
            public decimal precioventa { get; set; }
            public decimal preciocompra { get; set; }
            public decimal? margenutilidad { get; set; }
            public string? unidadmedida { get; set; }
            public decimal stockactual { get; set; }
            public decimal stockminimo { get; set; }
            public decimal stockmaximo { get; set; }
            public bool afectoigv { get; set; }
            public bool activo { get; set; }
            public int total { get; set; }
        }

        private class ProductoDetalleDtoDb
        {
            public int id { get; set; }
            public int empresaid { get; set; }
            public string codigo { get; set; }
            public string? codigobarras { get; set; }
            public string nombre { get; set; }
            public string? descripcion { get; set; }
            public int? categoriaid { get; set; }
            public string? categoria { get; set; }
            public int? marcaid { get; set; }
            public string? marca { get; set; }
            public int? lineaid { get; set; }
            public string? linea { get; set; }
            public bool afectoigv { get; set; }
            public int? cuentaventaid { get; set; }
            public string? cuentaventacodigo { get; set; }
            public int? cuentacostoid { get; set; }
            public string? cuentacostocodigo { get; set; }
            public string? codigosunat { get; set; }
            public bool requiereinspeccion { get; set; }
            public decimal preciocompra { get; set; }
            public decimal? margenutilidad { get; set; }
            public decimal precioventa { get; set; }
            public int? monedaid { get; set; }
            public string? moneda { get; set; }
            public int? unidadmedidaid { get; set; }
            public string? unidadmedida { get; set; }
            public decimal stockminimo { get; set; }
            public decimal stockmaximo { get; set; }
            public decimal stockactual { get; set; }
            public string? ubicacionfisica { get; set; }
            public bool margeninventario { get; set; }
            public bool margenlotes { get; set; }
            public bool margenseries { get; set; }
            public bool margenvencimiento { get; set; }
            public string? imagenurl { get; set; }
            public bool activo { get; set; }
            public bool sujetoretencion { get; set; }
            public int? cuentacompraid { get; set; }
            public string? cuentacompracodigo { get; set; }
            public int? cuentainventarioid { get; set; }
            public string? cuentainventariocodigo { get; set; }
        }

        private class CategoriaDtoDb
        {
            public int id { get; set; }
            public string nombre { get; set; }
            public string? descripcion { get; set; }
        }

        private class MarcaDtoDb
        {
            public int id { get; set; }
            public string nombre { get; set; }
        }

        private class UnidadMedidaDtoDb
        {
            public int id { get; set; }
            public string codigo { get; set; }
            public string nombre { get; set; }
        }
        private class MonedaDtoDb
        {
            public int id { get; set; }
            public string codigo { get; set; }
            public string nombre { get; set; }
            public string simbolo { get; set; }
        }

        private class PlanContableDtoDb
        {
            public int id { get; set; }
            public string codigo { get; set; }
            public string nombre { get; set; }
            public string tipocuenta { get; set; }
        }
    }
}
