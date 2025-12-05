using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;
using SistemaContable.Domain.ValueObjects;
using SistemaContable.Infrastructurxe.Data.Repositories.Implementations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class FacturaElectronicaRepository : IFacturaElectronicaRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<VentaRepository> _logger;

        public FacturaElectronicaRepository(
            IConfiguration configuration,
            ILogger<VentaRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no configurada");
            _logger = logger;
        }
        public async Task<SpResultado> InsertarFacturaElectronicaAsync(
            FacturaElectronicaDto factura, string usuario, string rucEmpresa)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                    
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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
            DateTime fechaDesde, DateTime fechaHasta,
            string rucCliente = null, string tipoDoc = null,
            string estadoDoc = null, string _RucEmpresa = null, int limite = 100, int offset = 0)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_fecha_desde = fechaDesde.Date,
                    p_fecha_hasta = fechaHasta.Date,
                    p_ruc_cliente = rucCliente,
                    p_tipo_doc = tipoDoc,
                    p_estado_doc = estadoDoc,
                    p_ruc_empresa = _RucEmpresa,
                    p_limite = limite,
                    p_offset = offset
                };

                var result = await connection.QueryAsync<VentaListaDto>(
                    "SELECT * FROM \"suizaConta\".sp_listar_ventas(@p_fecha_desde, @p_fecha_hasta, @p_ruc_cliente, @p_tipo_doc, @p_estado_doc,@p_ruc_empresa, @p_limite, @p_offset)",
                    parameters,
                    commandTimeout: 60
                );

                return result.ToList();
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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM sp_anular_venta(@p_id_reg_venta, @p_motivo, @p_usuario)",
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

       
    }
}
