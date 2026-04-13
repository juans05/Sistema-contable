using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System.Text.Json;

namespace SistemaContable.Infrastructurxe.Data.Repositories.Implementations
{
    public class VentaRepository : IVentaRepository
    {
        private readonly NpgsqlDataSource _dataSource;
        private readonly ILogger<VentaRepository> _logger;

        public VentaRepository(
            NpgsqlDataSource dataSource,
            ILogger<VentaRepository> logger)
        {
            _dataSource = dataSource ?? throw new ArgumentNullException(nameof(dataSource));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ERegistroVenta> CrearConDetallesAsync(ERegistroVenta venta)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                // Convertir detalles a JSON
                var detallesJson = JsonSerializer.Serialize(venta.Detalles.Select(d => new
                {
                    numero_linea = d.NumeroLinea,
                    codigo_producto = d.CodigoProducto ?? "",
                    descripcion_producto = d.DescripcionProducto ?? "",
                    unidad_medida = d.UnidadMedida ?? "",
                    cantidad = d.Cantidad,
                    precio_unitario = d.PrecioUnitario,
                    precio_unitario_con_igv = d.PrecioUnitarioConIgv,
                    valor_venta = d.ValorVenta,
                    descuento = d.Descuento,
                    monto_igv = d.MontoIgv,
                    total_linea = d.TotalLinea,
                    tipo_afectacion_igv = d.TipoAfectacionIgv ?? "",
                    porcentaje_igv = d.PorcentajeIgv
                }));

                var parameters = new
                {
                    p_id_factura_electronica = venta.IdFacturaElectronica,
                    p_ruc_cliente = venta.RucCliente,
                    p_periodo = venta.Periodo,
                    p_rs_cliente = venta.RSCliente,
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
                    p_usuario_creacion = venta.UsuarioCreacion ?? "SYSTEM",
                    p_detalles = detallesJson
                };

                var result = await connection.QueryFirstOrDefaultAsync<SpCrearVentaResultado>(
                    "SELECT * FROM sp_crear_venta_con_detalles(@p_id_factura_electronica, @p_ruc_cliente, @p_periodo, @p_rs_cliente, @p_tipo_doc, @p_serie_doc, @p_num_doc, @p_fecha_emision, @p_fecha_vencimiento, @p_tip_cambio, @p_tipo_doc_cliente, @p_moneda, @p_sub_total, @p_imp_igv, @p_total_doc, @p_tip_opera_sunat, @p_usuario_creacion, @p_detalles::jsonb)",
                    parameters,
                    commandTimeout: 60
                );

                if (result?.OIdRegVenta == null)
                {
                    throw new Exception(result?.OMensaje ?? "Error al crear venta");
                }

                venta.IdRegVenta = result.OIdRegVenta.Value;
                _logger.LogInformation("Venta creada con ID: {IdVenta}", venta.IdRegVenta);

                return venta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando venta con detalles");
                throw;
            }
        }

        public async Task<bool> ExisteVentaPorDocumentoAsync(string tipo, string serie, string numero)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                return await connection.ExecuteScalarAsync<bool>(
                    "SELECT sp_existe_venta_por_documento(@p_tipo_doc, @p_serie_doc, @p_num_doc)",
                    new
                    {
                        p_tipo_doc = tipo,
                        p_serie_doc = serie,
                        p_num_doc = numero
                    },
                    commandTimeout: 10
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error verificando existencia de venta");
                return false;
            }
        }


        public async Task<ERegistroVenta> ObtenerPorIdAsync(int id)
        {
            try
            {
                await using var connection = await _dataSource.OpenConnectionAsync();

                var ventaData = await connection.QueryAsync<VentaPorIdResultado>(
                    "SELECT * FROM sp_obtener_venta_por_id(@p_id_reg_venta)",
                    new { p_id_reg_venta = id },
                    commandTimeout: 30
                );

                if (!ventaData.Any())
                    return null;

                var primera = ventaData.First();

                var venta = new ERegistroVenta
                {
                    IdRegVenta = primera.IdRegVenta,
                    IdFacturaElectronica = primera.IdFacturaElectronica,
                    RucCliente = primera.RucCliente,
                    Periodo = primera.Periodo,
                    RSCliente = primera.RSCliente,
                    TipoDoc = primera.TipoDoc,
                    SerieDoc = primera.SerieDoc,
                    NumDoc = primera.NumDoc,
                    FechaEmision = primera.FechaEmision,
                    FechaVencimiento = primera.FechaVencimiento,
                    TipCambio = primera.TipCambio,
                    TipoDocCliente = primera.TipoDocCliente,
                    Moneda = primera.Moneda,
                    SubTotal = primera.SubTotal,
                    ImpIgv = primera.ImpIgv,
                    TotalDoc = primera.TotalDoc,
                    EstadoDoc = primera.EstadoDoc,
                    TipOperaSunat = primera.TipOperaSunat,
                    CreatedAt = primera.CreatedAt,
                    Detalles = new List<ERegistroVentaDetalle>()
                };

                // Agregar detalles
                foreach (var item in ventaData.Where(d => d.DetalleId.HasValue))
                {
                    venta.Detalles.Add(new ERegistroVentaDetalle
                    {
                        IdDetalle = item.DetalleId.Value,
                        IdRegVenta = item.IdRegVenta,
                        NumeroLinea = item.DetalleNumeroLinea ?? 0,
                        CodigoProducto = item.DetalleCodigoProducto,
                        DescripcionProducto = item.DetalleDescripcion,
                        UnidadMedida = item.DetalleUnidadMedida,
                        Cantidad = item.DetalleCantidad ?? 0,
                        PrecioUnitario = item.DetallePrecioUnitario ?? 0,
                        PrecioUnitarioConIgv = item.DetallePrecioUnitarioConIgv ?? 0,
                        ValorVenta = item.DetalleValorVenta ?? 0,
                        Descuento = item.DetalleDescuento ?? 0,
                        MontoIgv = item.DetalleMontoIgv ?? 0,
                        TotalLinea = item.DetalleTotalLinea ?? 0,
                        TipoAfectacionIgv = item.DetalleTipoAfectacionIgv,
                        PorcentajeIgv = item.DetallePorcentajeIgv ?? 0
                    });
                }

                return venta;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo venta por ID {Id}", id);
                return null;
            }
        }
    }
}
