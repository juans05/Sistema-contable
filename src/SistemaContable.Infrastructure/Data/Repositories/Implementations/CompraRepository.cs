using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.Entities;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class CompraRepository : ICompraRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly NpgsqlConnection _externalConnection;
        private readonly NpgsqlTransaction _externalTransaction;
        private readonly ILogger<CompraRepository> _logger;

        public CompraRepository(NpgsqlDataSource dataSource, ILogger<CompraRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public CompraRepository(NpgsqlConnection externalConnection, NpgsqlTransaction externalTransaction, ILogger<CompraRepository> logger)
        {
            _externalConnection = externalConnection ?? throw new ArgumentNullException(nameof(externalConnection));
            _externalTransaction = externalTransaction ?? throw new ArgumentNullException(nameof(externalTransaction));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<SpResultado> InsertarFacturaCompraElectronicaAsync(
            FacturaCompraElectronicaDto facturaCompra, string usuario, string rucEmpresa)
        {
            try
            {
                var isExternal = _externalConnection != null;
                var connection = isExternal ? _externalConnection : await _dataSource.OpenConnectionAsync();

                try
                {
                    var parameters = new
                    {
                        p_serie = facturaCompra.Serie,
                        p_numero = facturaCompra.Numero,
                        p_numero_completo = facturaCompra.NumeroCompleto,
                        p_tipo_documento = facturaCompra.TipoDocumento,
                        p_fecha_emision = facturaCompra.FechaEmision,
                        p_fecha_vencimiento = facturaCompra.FechaVencimiento,
                        p_moneda = facturaCompra.Moneda,
                        p_monto_base = facturaCompra.MontoBase,
                        p_monto_igv = facturaCompra.MontoIgv,
                        p_monto_total = facturaCompra.MontoTotal,
                        p_xml_original = facturaCompra.XmlOriginal,
                        p_codigo_hash = facturaCompra.CodigoHash,
                        p_usuario_creacion = usuario,
                        p_estado = 1,
                        p_ruc_empresa = rucEmpresa
                    };

                    var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                        "SELECT * FROM \"suizaConta\".sp_insertar_factura_compra_electronica(@p_serie, @p_numero, @p_numero_completo, @p_tipo_documento, @p_fecha_emision, @p_fecha_vencimiento, @p_moneda," +
                        "                                                             @p_monto_base, @p_monto_igv, @p_monto_total, @p_xml_original, " +
                        "                                                             @p_codigo_hash, @p_usuario_creacion,@p_estado,@p_ruc_empresa)",
                        parameters,
                        transaction: isExternal ? _externalTransaction : null,
                        commandTimeout: 30
                    );

                    return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
                }
                finally
                {
                    if (!isExternal && connection != null) await connection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando factura de compra electrónica");
                return new SpResultado
                {
                    OExisteDuplicado = false,
                    OMensaje = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<SpResultado> InsertarRegistroCompraAsync(RegistroCompraDto compra, string usuario)
        {
            try
            {
                var isExternal = _externalConnection != null;
                var connection = isExternal ? _externalConnection : await _dataSource.OpenConnectionAsync();

                try
                {
                    var fecha = DateTime.Parse(compra.Periodo);
                    var parameters = new
                    {
                        p_id_factura_compra_electronica = compra.IdFacturaCompraElectronica,
                        p_ruc_prov = compra.IdRucProv,
                        p_periodo = fecha.ToString("yyyyMM"),
                        p_nombre_prov = compra.NombreProv,
                        p_tipo_doc = compra.TipDocumento,
                        p_serie_doc = compra.SerieDocumento,
                        p_num_doc = compra.NoDocumento,
                        p_fecha_emision = compra.FEmisc,
                        p_fecha_vencimiento = compra.FVcto,
                        p_tip_cambio = compra.TipCambio,
                        p_moneda = compra.Moneda,
                        p_sub_total = compra.SubTotal,
                        p_imp_igv = compra.ImpIgv,
                        p_total_doc = compra.TotalDoc,
                        p_tip_opera_sunat = compra.TipOperaSunat,
                        p_usuario_creacion = usuario,
                        p_estado = compra.estadoDocumento
                    };

                    var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                        "SELECT * FROM \"suizaConta\".sp_insertar_registro_compra(@p_id_factura_compra_electronica, @p_ruc_prov, @p_periodo, @p_nombre_prov, @p_tipo_doc, @p_serie_doc, @p_num_doc," +
                        " @p_fecha_emision, @p_fecha_vencimiento, @p_tip_cambio, @p_moneda, @p_sub_total, @p_imp_igv, @p_total_doc, @p_tip_opera_sunat, @p_usuario_creacion,@p_estado)",
                        parameters,
                        transaction: isExternal ? _externalTransaction : null,
                        commandTimeout: 30
                    );

                    return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
                }
                finally
                {
                    if (!isExternal && connection != null) await connection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando registro de compra");
                return new SpResultado
                {
                    OExisteDuplicado = false,
                    OMensaje = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<SpResultado> InsertarCompraDetalleAsync(
            int idRegCompra, RegistroCompraDetalleDto compraDetalle)
        {
            try
            {
                var isExternal = _externalConnection != null;
                var connection = isExternal ? _externalConnection : await _dataSource.OpenConnectionAsync();

                try
                {
                    var parameters = new
                    {
                        p_id_reg_compra = idRegCompra,
                        p_numero_linea = compraDetalle.NumeroLinea,
                        p_codigo_producto = compraDetalle.CodigoProducto,
                        p_descripcion_producto = compraDetalle.DescripcionProducto,
                        p_unidad_medida = compraDetalle.UnidadMedida,
                        p_cantidad = compraDetalle.Cantidad,
                        p_precio_unitario = compraDetalle.PrecioUnitario,
                        p_precio_unitario_con_igv = compraDetalle.PrecioUnitarioConIgv,
                        p_valor_venta = compraDetalle.ValorCompra,
                        p_descuento = compraDetalle.Descuento,
                        p_monto_igv = compraDetalle.MontoIgv,
                        p_total_linea = compraDetalle.TotalLinea,
                        p_tipo_afectacion_igv = compraDetalle.TipoAfectacionIgv,
                        p_porcentaje_igv = compraDetalle.PorcentajeIgv
                    };

                    var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                        "SELECT * FROM \"suizaConta\".sp_insertar_compra_detalle(@p_id_reg_compra, @p_numero_linea, @p_codigo_producto, @p_descripcion_producto, @p_unidad_medida, @p_cantidad, @p_precio_unitario, @p_precio_unitario_con_igv, @p_valor_venta, @p_descuento, @p_monto_igv, @p_total_linea, @p_tipo_afectacion_igv, @p_porcentaje_igv)",
                        parameters,
                        transaction: isExternal ? _externalTransaction : null,
                        commandTimeout: 30
                    );

                    return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
                }
                finally
                {
                    if (!isExternal && connection != null) await connection.DisposeAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando detalle de compra");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }

        public async Task<bool> ExisteFacturaCompraPorHashAsync(string hash, string ruc)
        {
            var isExternal = _externalConnection != null;
            var connection = isExternal ? _externalConnection : await _dataSource.OpenConnectionAsync();

            try
            {
                return await connection.ExecuteScalarAsync<bool>(
                    @"SELECT EXISTS(
                            SELECT 1 FROM ""suizaConta"".facturas_compras_electronicas 
                            WHERE codigo_hash = @Hash and estado=1 and  rucempresa=@ruc
                        )",
                    new { Hash = hash, ruc = ruc },
                    transaction: isExternal ? _externalTransaction : null,
                    commandTimeout: 10
                );
            }
            finally
            {
                if (!isExternal && connection != null) await connection.DisposeAsync();
            }
        }
        public async Task<List<VentaListaDto>> ListarComprasAsync(
           string fechaDesde, string fechaHasta,
           string rucProveedor = null, string tipoDoc = null,
           string estadoDoc = null, string _RucEmpresa = null,
           int limit = 10, int offset = 0, string filtro = null)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var sqlBuilder = new StringBuilder(@"
                    SELECT 
                        rc.id AS ""IdRegVenta"",
                        COALESCE(rc.serie_doc, '') || '-' || COALESCE(rc.num_doc, '') AS ""NumeroDocumento"",
                        rc.fecha_emision AS ""FechaEmision"",
                        rc.ruc_prov AS ""RucCliente"",
                        rc.nombre_prov AS ""RazonSocial"",
                        rc.moneda AS ""Moneda"",
                        rc.total_doc AS ""TotalDoc"",
                        rc.estado_documento::text AS ""EstadoDoc"",
                        '' AS ""EstadoSunat"",
                        '' AS ""NumeroFactura"",
                        0 AS ""CantidadItems""
                    FROM ""suizaConta"".registro_compras rc
                    WHERE rc.fecha_emision >= @FechaDesde::date 
                      AND rc.fecha_emision <= @FechaHasta::date
                ");

                var parameters = new DynamicParameters();
                parameters.Add("FechaDesde", fechaDesde);
                parameters.Add("FechaHasta", fechaHasta);
                parameters.Add("Limit", limit);
                parameters.Add("Offset", offset);

                if (!string.IsNullOrEmpty(rucProveedor)) 
                {
                    sqlBuilder.Append(" AND rc.ruc_prov = @RucProv");
                    parameters.Add("RucProv", rucProveedor);
                }

                if (!string.IsNullOrEmpty(tipoDoc)) 
                {
                    sqlBuilder.Append(" AND rc.tipo_doc = @TipoDoc");
                    parameters.Add("TipoDoc", tipoDoc);
                }

                if (!string.IsNullOrEmpty(estadoDoc)) 
                {
                    sqlBuilder.Append(" AND rc.estado_documento::text = @EstadoDoc");
                    parameters.Add("EstadoDoc", estadoDoc);
                }

                if (!string.IsNullOrEmpty(filtro))
                {
                     sqlBuilder.Append(@" AND (
                        rc.nombre_prov ILIKE @Filtro 
                        OR rc.ruc_prov ILIKE @Filtro 
                        OR (COALESCE(rc.serie_doc, '') || '-' || COALESCE(rc.num_doc, '')) ILIKE @Filtro
                    )");
                    parameters.Add("Filtro", $"%{filtro}%");
                }
                
                sqlBuilder.Append(" ORDER BY rc.fecha_emision DESC LIMIT @Limit OFFSET @Offset");

                var result = await connection.QueryAsync<VentaListaDto>(sqlBuilder.ToString(), parameters);

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando compras");
                return new List<VentaListaDto>();
            }
        }

        public async Task<VentaCompletaDto> ObtenerCompraCompletaAsync(int idRegCompra)
        {
             try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                // Simple Get
                var sql = @"SELECT 
                                id AS IdRegVenta,
                                id_factura_compra_electronica AS IdFacturaElectronica,
                                COALESCE(serie_doc, '') || '-' || COALESCE(num_doc, '') AS NumeroDocumento,
                                fecha_emision AS FechaEmision,
                                ruc_prov AS RucCliente,
                                nombre_prov AS RazonSocialCliente,
                                moneda AS Moneda,
                                sub_total AS SubTotal,
                                imp_igv AS ImpIgv,
                                total_doc AS TotalDoc
                            FROM ""suizaConta"".registro_compras 
                            WHERE id = @Id";
                
                var compra = await connection.QueryFirstOrDefaultAsync<VentaCompletaDto>(sql, new { Id = idRegCompra });
                
                if(compra != null) {
                    // Get Details
                     var sqlDet = @"SELECT 
                                    numero_linea AS NumeroLinea,
                                    codigo_producto AS CodigoProducto,
                                    descripcion_producto AS Descripcion,
                                    cantidad AS Cantidad,
                                    precio_unitario AS Precio,
                                    total_linea AS Total
                                   FROM ""suizaConta"".registro_compras_detalles
                                   WHERE id_reg_compra = @Id";
                     
                     var detalles = await connection.QueryAsync<DetalleVentaDto>(sqlDet, new { Id = idRegCompra });
                     compra.Detalles = detalles.ToList();
                }

                return compra;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo compra completa");
                return null;
            }
        }

        public async Task<string> ObtenerXmlCompraPorIdAsync(int idRegCompra)
        {
             try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var xml = await connection.QueryFirstOrDefaultAsync<string>(
                    @"SELECT f.xml_original 
                      FROM ""suizaConta"".facturas_compras_electronicas f
                      INNER JOIN ""suizaConta"".registro_compras c ON c.id_factura_compra_electronica = f.id
                      WHERE c.id = @Id",
                    new { Id = idRegCompra },
                    commandTimeout: 10
                );

                return xml;
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error obteniendo XML de compra {Id}", idRegCompra);
                return null;
            }
        }
        public async Task<ERegistroCompra> ObtenerCompraPorIdAsync(int idRegCompra)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var sql = @"SELECT 
                                id AS IdRegCompras,
                                id_ruc_prov AS IdRucProveedor,
                                periodo AS Periodo,
                                nombre_prov AS NombreProv,
                                moneda AS Moneda,
                                tip_cambio AS TipCambio,
                                tipo_doc AS TipDocumento,
                                serie_doc AS SerieDocumento,
                                num_doc AS NoDocumento,
                                fecha_emision AS FEmisc,
                                fecha_vencimiento AS FVcto,
                                sub_total AS SubTotal,
                                imp_igv AS ImpIgv,
                                total_doc AS TotalDoc,
                                estado_documento AS EstadoDocumento
                            FROM ""suizaConta"".registro_compras 
                            WHERE id = @Id";

                return await connection.QueryFirstOrDefaultAsync<ERegistroCompra>(sql, new { Id = idRegCompra });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo entidad compra {Id}", idRegCompra);
                return null;
            }
        }
        public async Task<List<SistemaContable.Application.DTOs.Sire.SireCompraDto>> ListarComprasParaSire(string periodo, string rucEmpresa)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                // Lógica simplificada: Asumimos todo como "Base Imponible Gravada - Destino Gravado" (DG)
                // En un sistema real, se debería determinar el destino (Gravado, Mixto, No Gravado) según la cuenta contable o configuración.
                var sql = @"
                    SELECT 
                        c.id AS Id,
                        '' AS RucEmpresa, -- Se llenará en servicio o join
                        '' AS RazonSocialEmpresa, 
                        c.periodo AS Periodo,
                        '' AS Car, 
                        c.fecha_emision AS FechaEmision,
                        c.fecha_vencimiento AS FechaVencimiento,
                        c.tipo_doc AS TipoComprobante,
                        c.serie_doc AS Serie,
                        '' AS AnioEmisionDua,
                        c.num_doc AS Numero,
                        '6' AS TipoDocProveedor, -- Default RUC
                        c.ruc_prov AS RucProveedor,
                        c.nombre_prov AS RazonSocialProveedor,
                        
                        -- Asumimos todo a Destino Gravado (DG) por ahora
                        CASE WHEN c.imp_igv > 0 THEN c.sub_total ELSE 0.00 END AS BaseImponibleGravadaDG,
                        c.imp_igv AS IgvDG,
                        
                        0.00 AS BaseImponibleGravadaDM,
                        0.00 AS IgvDM,
                        
                        0.00 AS BaseImponibleGravadaDNG,
                        0.00 AS IgvDNG,
                        
                        CASE WHEN c.imp_igv = 0 THEN c.sub_total ELSE 0.00 END AS MontoExonerado,
                        0.00 AS MontoInafecto,
                        0.00 AS MontoIsc,
                        0.00 AS MontoIcbper,
                        0.00 AS OtrosTributos,
                        c.total_doc AS TotalComprobante,
                        
                        c.moneda AS Moneda,
                        c.tip_cambio AS TipoCambio
                        
                    FROM ""suizaConta"".registro_compras c
                    WHERE c.periodo = @Periodo 
                      
                    ORDER BY c.fecha_emision ASC, c.serie_doc ASC, c.num_doc ASC";
                    // TODO: Filtrar por rucEmpresa si la tabla registro_compras tuviera esa columna bien poblada. 
                    // Asumimos que el filtrado por tenant se hace antes o la tabla es compartida (FIXME).

                var result = await connection.QueryAsync<SistemaContable.Application.DTOs.Sire.SireCompraDto>(sql, new { Periodo = periodo });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando compras para SIRE");
                return new List<SistemaContable.Application.DTOs.Sire.SireCompraDto>();
            }
        }
    }
}
