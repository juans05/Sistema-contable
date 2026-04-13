using Dapper;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;
using System.Text;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class FacturaElectronicaRepository : IFacturaElectronicaRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<FacturaElectronicaRepository> _logger;

        public FacturaElectronicaRepository(
            NpgsqlDataSource dataSource,
            ILogger<FacturaElectronicaRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<SpResultado> InsertarFacturaElectronicaAsync(
            FacturaElectronicaDto factura, string usuario, string rucEmpresa)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_serie = factura.Serie,
                    p_numero = factura.Numero,
                    p_numero_completo = factura.NumeroCompleto,
                    p_tipo_documento = factura.TipoDocumento,
                    p_fecha_emision = factura.FechaEmision,
                    p_fecha_vencimiento = factura.FechaVencimiento,
                    p_moneda = factura.Moneda,
                    p_monto_base = factura.MontoBase,
                    p_monto_igv = factura.MontoIgv,
                    p_monto_total = factura.MontoTotal,
                    p_xml_original = factura.XmlOriginal,
                    p_codigo_hash = factura.CodigoHash,
                    p_usuario_creacion = usuario,
                    p_estado =1,
                    p_ruc_empresa = rucEmpresa
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM \"suizaConta\".sp_insertar_factura_electronica(@p_serie, @p_numero, @p_numero_completo, @p_tipo_documento, @p_fecha_emision, @p_fecha_vencimiento, @p_moneda," +
                    "                                                             @p_monto_base, @p_monto_igv, @p_monto_total, @p_xml_original, " +
                    "                                                             @p_codigo_hash, @p_usuario_creacion,@p_estado,@p_ruc_empresa)",
                    parameters,
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando factura electrónica");
                return new SpResultado
                {
                    OExisteDuplicado = false,
                    OMensaje = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<SpResultado> InsertarRegistroVentaAsync(RegistroVentaDto venta, string usuario, string rucEmpresa,int estado)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                var fecha = DateTime.Parse(venta.Periodo);
                var parameters = new
                {
                    p_id_factura_electronica = venta.IdFacturaElectronica,
                    p_ruc_cliente = venta.RucCliente,
                    p_periodo = fecha.ToString("yyyyMM"),
                    p_rs_cliente = venta.RsCliente,
                    p_tipo_doc = venta.TipoDoc,
                    p_serie_doc = venta.SerieDoc,
                    p_num_doc = venta.NumDoc,
                    p_fecha_emision = venta.FechaEmision,
                    p_fecha_vencimiento = venta.FechaVencimiento,
                    p_tip_cambio = venta.TipCambio,
                    p_tipo_doc_cliente = venta.TipoDocCliente,
                    p_moneda = venta.Moneda,
                    p_sub_total = venta.SubTotal,
                    p_imp_igv = venta.ImpIgv,
                    p_total_doc = venta.TotalDoc,
                    p_tip_opera_sunat = venta.TipOperaSunat,
                    p_usuario_creacion = usuario,
                    p_estado =  estado,
                    p_ruc_empresa= rucEmpresa
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM \"suizaConta\".sp_insertar_registro_venta(@p_id_factura_electronica, @p_ruc_cliente, @p_periodo, @p_rs_cliente, @p_tipo_doc, @p_serie_doc, @p_num_doc," +
                    " @p_fecha_emision, @p_fecha_vencimiento, @p_tip_cambio, @p_tipo_doc_cliente, @p_moneda, @p_sub_total, @p_imp_igv, @p_total_doc, @p_tip_opera_sunat, @p_usuario_creacion,@p_estado, @p_ruc_empresa)",
                    parameters,
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
                catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando registro de venta");
                return new SpResultado
                {
                    OExisteDuplicado = false,
                    OMensaje = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<SpResultado> InsertarVentaDetalleAsync(
            int idRegVenta, RegistroVentaDetalleDto detalle)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();
                    
                var parameters = new
                {
                    p_id_reg_venta = idRegVenta,
                    p_numero_linea = detalle.NumeroLinea,
                    p_codigo_producto = detalle.CodigoProducto,
                    p_descripcion_producto = detalle.DescripcionProducto,
                    p_unidad_medida = detalle.UnidadMedida,
                    p_cantidad = detalle.Cantidad,
                    p_precio_unitario = detalle.PrecioUnitario,
                    p_precio_unitario_con_igv = detalle.PrecioUnitarioConIgv,
                    p_valor_venta = detalle.ValorVenta,
                    p_descuento = detalle.Descuento,
                    p_monto_igv = detalle.MontoIgv,
                    p_total_linea = detalle.TotalLinea,
                    p_tipo_afectacion_igv = detalle.TipoAfectacionIgv,
                    p_porcentaje_igv = detalle.PorcentajeIgv
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM \"suizaConta\".sp_insertar_venta_detalle(@p_id_reg_venta, @p_numero_linea, @p_codigo_producto, @p_descripcion_producto, @p_unidad_medida, @p_cantidad, @p_precio_unitario, @p_precio_unitario_con_igv, @p_valor_venta, @p_descuento, @p_monto_igv, @p_total_linea, @p_tipo_afectacion_igv, @p_porcentaje_igv)",
                    parameters,
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando detalle de venta");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }

        public async Task<bool> VerificarDuplicadoHashAsync(string hash)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                return await connection.ExecuteScalarAsync<bool>(
                    "SELECT sp_verificar_duplicado_hash(@p_codigo_hash)",
                    new { p_codigo_hash = hash },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando duplicado");
                return false;
            }
        }

        public async Task<VentaCompletaDto> ObtenerVentaCompletaAsync(int idRegVenta)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var ventaData = await connection.QueryAsync<VentaCompletaRaw>(
                    "SELECT * FROM \"suizaConta\".sp_obtener_venta_completa(@p_id_reg_venta)",
                    new { p_id_reg_venta = idRegVenta },
                    commandTimeout: 30
                );

                if (!ventaData.Any())
                    return null;

                var primera = ventaData.First();
                return new VentaCompletaDto
                {
                    IdRegVenta = primera.IdRegVenta,
                    IdFacturaElectronica = primera.IdFacturaElectronica,
                    NumeroDocumento = primera.NumeroDocumento,
                    FechaEmision = primera.FechaEmision,
                    RucCliente = primera.RucCliente,
                    RazonSocialCliente = primera.RazonSocialCliente,
                    Moneda = primera.Moneda,
                    SubTotal = primera.SubTotal,
                    ImpIgv = primera.ImpIgv,
                    TotalDoc = primera.TotalDoc,
                    EstadoDoc = primera.EstadoDoc,
                    EstadoSunat = primera.EstadoSunat,
                    NumeroFacturaElectronica = primera.NumeroFacturaElectronica,
                    Detalles = ventaData
                        .Where(d => d.DetalleId.HasValue)
                        .Select(d => new DetalleVentaDto
                        {
                            NumeroLinea = d.DetalleNumeroLinea ?? 0,
                            CodigoProducto = d.DetalleProducto,
                            Descripcion = d.DetalleDescripcion,
                            Cantidad = d.DetalleCantidad ?? 0,
                            Precio = d.DetallePrecio ?? 0,
                            Total = d.DetalleTotal ?? 0
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo venta completa");
                return null;
            }
        }

        public async Task<List<VentaListaDto>> ListarVentasAsync(
            string fechaDesde, string fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null, 
            int limite = 100, int offset = 0, string filtro = null)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var sql = new StringBuilder(@"
                    SELECT 
                        v.id_reg_venta AS ""IdRegVenta"",
                        v.serie_doc || '-' || v.num_doc AS ""NumeroDocumento"",
                        v.fecha_emision AS ""FechaEmision"",
                        v.ruc_cliente AS ""RucCliente"",
                        v.r_s_cliente AS ""RazonSocial"",
                        v.moneda AS ""Moneda"",
                        v.total_doc AS ""TotalDoc"",
                        CAST(v.estado_documento AS TEXT) AS ""EstadoDoc"",
                        f.estado_sunat AS ""EstadoSunat"",
                        f.serie || '-' || f.numero AS ""NumeroFactura""
                    FROM ""suizaConta"".registro_venta v
                    LEFT JOIN ""suizaConta"".facturas_electronicas f ON v.id_factura_electronica = f.id_factura_electronica
                    WHERE v.rucempresa = @RucEmpresa
                      AND v.fecha_emision >= @FechaDesde::date 
                      AND v.fecha_emision <= @FechaHasta::date
                ");

                var parameters = new DynamicParameters();
                parameters.Add("RucEmpresa", _RucEmpresa);
                parameters.Add("FechaDesde", fechaDesde);
                parameters.Add("FechaHasta", fechaHasta);
                parameters.Add("Limit", limite);
                parameters.Add("Offset", offset);

                if (!string.IsNullOrEmpty(rucCliente))
                {
                    sql.Append(" AND v.ruc_cliente = @RucCliente");
                    parameters.Add("RucCliente", rucCliente);
                }

                if (!string.IsNullOrEmpty(tipoDoc))
                {
                    sql.Append(" AND v.tipo_doc = @TipoDoc");
                    parameters.Add("TipoDoc", tipoDoc);
                }
                
                // Filtro de texto general (Cliente, RUC o Nro Doc)
                if (!string.IsNullOrEmpty(filtro))
                {
                    sql.Append(@" AND (
                        v.r_s_cliente ILIKE @Filtro 
                        OR v.ruc_cliente ILIKE @Filtro 
                        OR (v.serie_doc || '-' || v.num_doc) ILIKE @Filtro
                    )");
                    parameters.Add("Filtro", $"%{filtro}%");
                }

                sql.Append(" ORDER BY v.fecha_emision DESC LIMIT @Limit OFFSET @Offset");

                var result = await connection.QueryAsync<VentaListaDto>(sql.ToString(), parameters);
                var lista = result.ToList();
                _logger.LogInformation("Repository: ListarVentas completado. RucEmpresa: {Ruc}, Encontrados: {Count}", _RucEmpresa, lista.Count);
                return lista;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando ventas");
                return new List<VentaListaDto>();
            }
        }



        public async Task<SpResultado> ActualizarEstadoSunatAsync(
            int idFactura, string estado, string codigo, string mensaje,
            string cdr = null, string xmlFirmado = null)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var parameters = new
                {
                    p_id_factura_electronica = idFactura,
                    p_estado_sunat = estado,
                    p_codigo_respuesta = codigo,
                    p_mensaje_sunat = mensaje,
                    p_cdr_sunat = cdr,
                    p_xml_firmado = xmlFirmado
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM \"suizaConta\".sp_actualizar_estado_sunat(@p_id_factura_electronica, @p_estado_sunat, @p_codigo_respuesta, @p_mensaje_sunat, @p_cdr_sunat, @p_xml_firmado)",
                    parameters,
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando estado SUNAT");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }


        public async Task<bool> ExisteFacturaPorHashAsync(string hash,string ruc)
        {
            await using var connection = await _dataSource.OpenConnectionAsync();

            return await connection.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS(
                        SELECT 1 FROM ""suizaConta"".facturas_electronicas 
                        WHERE codigo_hash = @Hash and estado=1 and  rucempresa=@ruc
                    )",
                new { Hash = hash, ruc  = ruc },
                commandTimeout: 10
            );
        }


        public async Task<SpResultado> AnularVentaAsync(int idRegVenta, string motivo, string usuario)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    @"SELECT * FROM ""suizaConta"".sp_anular_venta(@p_id_reg_venta, @p_motivo, @p_usuario)",
                    new
                    {
                        p_id_reg_venta = idRegVenta,
                        p_motivo = motivo,
                        p_usuario = usuario
                    },
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anulando venta");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }

        public async Task<FacturaElectronicaDto> CrearAsync(FacturaElectronicaDto factura, string usuario, string rucEmpresa)
        {
            var resultado = await InsertarFacturaElectronicaAsync(factura, usuario, rucEmpresa);

            if (resultado.OIdFactura.HasValue)
            {
                factura.IdFacturaElectronica = resultado.OIdFactura.Value;
                return factura;
            }

            throw new Exception(resultado.OMensaje ?? "Error al crear factura");
        }

    
        public async Task<string> ObtenerXmlPorVentaIdAsync(int idRegVenta)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var xml = await connection.QueryFirstOrDefaultAsync<string>(
                    @"SELECT f.xml_original 
                      FROM ""suizaConta"".facturas_electronicas f
                      INNER JOIN ""suizaConta"".registro_venta v ON v.id_factura_electronica = f.id_factura_electronica
                      WHERE v.id_reg_venta = @Id",
                    new { Id = idRegVenta },
                    commandTimeout: 10
                );

                return xml;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo XML de venta {Id}", idRegVenta);
                return null;
            }
        }
        public async Task<List<SistemaContable.Application.DTOs.Sire.SireVentaDto>> ListarVentasParaSire(string periodo, string rucEmpresa)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var sql = @"
                    SELECT 
                        v.id_reg_venta AS Id,
                        v.rucempresa AS RucEmpresa,
                        '' AS RazonSocialEmpresa, 
                        v.periodo AS Periodo,
                        '' AS Car, 
                        v.fecha_emision AS FechaEmision,
                        v.fecha_vencimiento AS FechaVencimiento,
                        v.tipo_doc AS TipoComprobante,
                        v.serie_doc AS Serie,
                        v.num_doc AS Numero,
                        v.tipo_doc_cliente AS TipoDocCliente,
                        v.ruc_cliente AS RucCliente,
                        v.r_s_cliente AS RazonSocialCliente,
                        0.00 AS ValoFacturadoExportacion,
                        CASE WHEN v.imp_igv > 0 THEN v.sub_total ELSE 0.00 END AS BaseImponibleGravada,
                        v.imp_igv AS MontoIgv,
                        CASE WHEN v.imp_igv = 0 AND v.tipo_doc <> '07' THEN v.sub_total ELSE 0.00 END AS MontoExonerado,
                        0.00 AS MontoInafecto,
                        0.00 AS MontoIsc,
                        0.00 AS MontoIcbper,
                        0.00 AS OtrosTributos,
                        v.total_doc AS TotalComprobante,
                        v.moneda AS Moneda,
                        v.tip_cambio AS TipoCambio
                    FROM ""suizaConta"".registro_venta v
                    WHERE v.periodo = @Periodo 
                      AND v.rucempresa = @RucEmpresa
                    ORDER BY v.fecha_emision ASC, v.serie_doc ASC, v.num_doc ASC";

                var result = await connection.QueryAsync<SistemaContable.Application.DTOs.Sire.SireVentaDto>(sql, new { Periodo = periodo, RucEmpresa = rucEmpresa });
                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando ventas para SIRE");
                return new List<SistemaContable.Application.DTOs.Sire.SireVentaDto>();
            }
        }
    }
}
