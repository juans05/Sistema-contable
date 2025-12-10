using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dapper;
using Microsoft.AspNetCore.Mvc.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Models;

namespace SistemaContable.Infrastructure.Data.Repositories.Implementations
{
    public class CompraRepository : ICompraRepository
    {
        private readonly string _connectionString;
        private readonly ILogger<CompraRepository> _logger;

        public CompraRepository(IConfiguration configuration, ILogger<CompraRepository> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string no configurada");
            _logger = logger;
        }

        public async Task<SpResultado> InsertarFacturaCompraElectronicaAsync(
            FacturaCompraElectronicaDto facturaCompra, string usuario, string rucEmpresa)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
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

        public async Task<SpResultado> InsertarRegistroCompraAsync(RegistroCompraDto compra, string usuario, string rucEmpresa)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
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
                    p_estado = compra.estadoDocumento,
                    p_ruc_empresa = rucEmpresa
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                    "SELECT * FROM \"suizaConta\".sp_insertar_registro_compra(@p_id_factura_compra_electronica, @p_ruc_prov, @p_periodo, @p_nombre_prov, @p_tipo_doc, @p_serie_doc, @p_num_doc," +
                    " @p_fecha_emision, @p_fecha_vencimiento, @p_tip_cambio, @p_moneda, @p_sub_total, @p_imp_igv, @p_total_doc, @p_tip_opera_sunat, @p_usuario_creacion,@p_estado, @p_ruc_empresa)",
                    parameters,
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
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
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

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
                    commandTimeout: 30
                );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error insertando detalle de compra");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }

        public async Task<bool> ExisteFacturaCompraPorHashAsync(string hash, string ruc)
        {
            await using var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();

            return await connection.ExecuteScalarAsync<bool>(
                @"SELECT EXISTS(
                        SELECT 1 FROM ""suizaConta"".facturas_compras_electronicas 
                        WHERE codigo_hash = @Hash and estado=1 and  rucempresa=@ruc
                    )",
                new { Hash = hash, ruc = ruc },
                commandTimeout: 10
            );
        }

        public async Task<List<CompraListaDto>> ListarComprasAsync(string fechaDesde, string fechaHasta, string rucProveedor = null, string tipoDoc = null, string estadoDoc = null, string _RucEmpresa = null, int limite = 100, int offset = 0)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var parameters = new
                {
                    p_fecha_desde = fechaDesde,
                    p_fecha_hasta = fechaHasta,
                    p_ruc_proveedor = rucProveedor,
                    p_tipo_doc = tipoDoc,
                    p_estado_doc = estadoDoc,
                    p_ruc_empresa = _RucEmpresa,
                    p_limite = limite,
                    p_offset = offset
                };

                var result = await connection.QueryAsync<CompraListaDto>(
                    "SELECT * FROM \"suizaConta\".sp_listar_compras(@p_fecha_desde, @p_fecha_hasta, @p_ruc_proveedor, @p_tipo_doc, @p_estado_doc,@p_ruc_empresa, @p_limite, @p_offset)",
                    parameters,
                    commandTimeout: 60
                );

                return result.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listando compras");
                return new List<CompraListaDto>();
            }
        }

        public async Task<SpResultado> AnularCompraAsync(int idRegCompras, string motivo, string usuario)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var result = await connection.QueryFirstOrDefaultAsync<SpResultado>(
                        @"SELECT * FROM ""suizaConta"".sp_anular_compra(@p_id_reg_compras, @p_motivo, @p_usuario)",
                        new
                        {
                            p_id_reg_compras = idRegCompras,
                            p_motivo = motivo,
                            p_usuario = usuario
                        },
                        commandTimeout: 30
                    );

                return result ?? new SpResultado { OMensaje = "Error: No se obtuvo respuesta del SP" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error anulando compra");
                return new SpResultado { OMensaje = $"Error: {ex.Message}" };
            }
        }

        public async Task<CompraCompletaDto> ObtenerCompraPorIdAsync(int id)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                var compraData = await connection.QueryAsync<CompraCompletaRaw>(
                        "SELECT * FROM \"suizaConta\".sp_obtener_compra_completa(@p_id_reg_compras)",
                        new { p_id_reg_compras = id },
                        commandTimeout: 30
                    );

                if (!compraData.Any())
                    return null;

                var primera = compraData.First();

                return new CompraCompletaDto
                {
                    IdRegCompras = primera.IdRegCompras,
                    IdFacturaCompraElectronica = primera.IdFacturaCompraElectronica,
                    NumeroDocumento = primera.NumeroDocumento,
                    FechaEmision = primera.FechaEmision,
                    RucProveedor = primera.RucProveedor,
                    NombreProveedor = primera.NombreProveedor,
                    Moneda = primera.Moneda,
                    SubTotal = primera.SubTotal,
                    ImpIgv = primera.ImpIgv,
                    TotalDoc = primera.TotalDoc,
                    EstadoDoc = primera.EstadoDoc,
                    EstadoSunat = primera.EstadoSunat,
                    NumeroFacturaCompraElectronica = primera.NumeroFacturaElectronica,
                    Detalles = compraData
                        .Where(d => d.DetalleId.HasValue)
                        .Select(d => new DetalleCompraDto
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
            catch (Exception)
            {

                throw;
            }
        }
    }
}
