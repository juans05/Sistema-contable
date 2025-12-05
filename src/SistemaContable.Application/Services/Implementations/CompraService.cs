using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Common.Helpers;
using SistemaContable.Domain.Models;

namespace SistemaContable.Application.Services.Implementations
{
    public class CompraService : ICompraService
    {
        private readonly ICompraRepository _compraRepository;
        private readonly ILogger<CompraService> _logger;

        public CompraService(ICompraRepository compraRepository, ILogger<CompraService> logger)
        {
            _compraRepository = compraRepository;
            _logger = logger;
        }

        public async Task<ProcesarXmlCompraRespondeDto> ProcesarXmlYRegistrarCompraAsync(List<IFormFile> archivosXml, string usuario, string ruc)
        {
            var response = new ProcesarXmlCompraRespondeDto
            {
                Resultados = new List<ResultadoCompraProcesamiento>()
            };
            foreach (var archivo in archivosXml)
            {
                var resultado = new ResultadoCompraProcesamiento
                {
                    NombreArchivo = archivo.FileName
                };

                try
                {
                    // 1. Leer XML
                    using var stream = archivo.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var xmlContent = await reader.ReadToEndAsync();

                    // 2. Calcular hash
                    var hash = Varios.CalcularHash(xmlContent);

                    // 3. Verificar duplicados
                    if (await _compraRepository.ExisteFacturaCompraPorHashAsync(hash, ruc))
                    {
                        resultado.Procesado = false;
                        resultado.Error = "Documento duplicado (hash ya existe)";
                        response.Resultados.Add(resultado);
                        continue;
                    }

                    // 4. Parsear XML
                    var datosXml = ParsearFacturaXml(Varios.NormalizarXml(xmlContent));

                    // 5. Insertar factura electrónica
                    var facturaCompraDto = new FacturaCompraElectronicaDto
                    {
                        Serie = datosXml.Serie,
                        Numero = datosXml.Numero,
                        NumeroCompleto = datosXml.NumeroCompleto,
                        TipoDocumento = datosXml.TipoDocumento,
                        FechaEmision = datosXml.FechaEmision,
                        FechaVencimiento = datosXml.FechaVencimiento,
                        Moneda = datosXml.Moneda,
                        MontoBase = datosXml.MontoBase,
                        MontoIgv = datosXml.MontoIgv,
                        MontoTotal = datosXml.MontoTotal,
                        XmlOriginal = XmlCompressor.Compress(Varios.LimpiarXml(Varios.NormalizarXml(xmlContent))),
                        CodigoHash = hash,
                        RucEmpresa = ruc
                    };

                    var resultadoFacturaCompra = await _compraRepository.InsertarFacturaCompraElectronicaAsync(
                        facturaCompraDto, usuario, ruc);

                    if (resultadoFacturaCompra.OExisteDuplicado || !resultadoFacturaCompra.OIdFactura.HasValue)
                    {
                        resultado.Procesado = false;
                        resultado.Error = resultadoFacturaCompra.OMensaje;
                        response.Resultados.Add(resultado);
                        continue;
                    }

                    var idFacturaCompraElectronica = resultadoFacturaCompra.OIdFactura.Value;

                    // 6. Insertar registro de venta
                    var compraDto = new RegistroCompraDto
                    {
                        IdFacturaCompraElectronica = idFacturaCompraElectronica,
                        IdRucProv = datosXml.ClienteRuc,
                        Periodo = datosXml.FechaEmision,
                        NombreProv = datosXml.ClienteRazonSocial,
                        TipDocumento = datosXml.TipoDocumento,
                        SerieDocumento = datosXml.Serie,
                        NoDocumento = datosXml.Numero,
                        FEmisc = datosXml.FechaEmision,
                        FVcto = datosXml.FechaVencimiento,
                        TipCambio = datosXml.TipoCambio,
                        Moneda = datosXml.Moneda,
                        SubTotal = datosXml.MontoBase,
                        ImpIgv = datosXml.MontoIgv,
                        TotalDoc = datosXml.MontoTotal,
                        TipOperaSunat = datosXml.TipoOperacion,
                    };

                    var resultadoCompra = await _compraRepository.InsertarRegistroCompraAsync(compraDto, usuario);

                    if (resultadoCompra.OExisteDuplicado || !resultadoCompra.OIdRegCompra.HasValue)
                    {
                        resultado.Procesado = false;
                        resultado.Error = resultadoCompra.OMensaje;
                        response.Resultados.Add(resultado);
                        continue;
                    }

                    var idRegCompra = resultadoCompra.OIdRegCompra.Value;

                    // 7. Insertar detalles
                    foreach (var det in datosXml.Detalles)
                    {
                        var detalleDto = new RegistroCompraDetalleDto
                        {
                            NumeroLinea = det.NumeroLinea,
                            CodigoProducto = det.CodigoProducto,
                            DescripcionProducto = det.Descripcion,
                            UnidadMedida = det.UnidadMedida,
                            Cantidad = det.Cantidad,
                            PrecioUnitario = det.PrecioUnitario,
                            PrecioUnitarioConIgv = det.PrecioUnitarioConIgv,
                            ValorCompra = det.ValorVenta,
                            Descuento = 0,
                            MontoIgv = det.MontoIgv,
                            TotalLinea = det.TotalLinea,
                            TipoAfectacionIgv = det.TipoAfectacionIgv,
                            PorcentajeIgv = 18.00m
                        };

                        await _compraRepository.InsertarCompraDetalleAsync(idRegCompra, detalleDto);
                    }

                    // 8. Resultado exitoso
                    resultado.Procesado = true;
                    resultado.NumeroDocumento = datosXml.NumeroCompleto;
                    resultado.IdCompra = idRegCompra;
                    resultado.IdFacturaCompraElectronica = idFacturaCompraElectronica;
                    response.ComprasRegistradas++;

                    _logger.LogInformation(
                        "Compra procesada exitosamente: {NumeroDocumento}, IdCompra: {IdCompra}",
                        datosXml.NumeroCompleto, idRegCompra);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando archivo {Archivo}", archivo.FileName);
                    resultado.Procesado = false;
                    resultado.Error = $"Error: {ex.Message}";
                }

                response.Resultados.Add(resultado);
            }

            response.Exito = response.ComprasRegistradas > 0;
            response.Mensaje = $"Procesados {response.ComprasRegistradas} de {archivosXml.Count} archivos";

            return response;
        }

        private DatosFacturaXml ParsearFacturaXml(string xmlContent)
        {
            XNamespace cbc = "urn:oasis:names:specification:ubl:schema:xsd:CommonBasicComponents-2";
            XNamespace cac = "urn:oasis:names:specification:ubl:schema:xsd:CommonAggregateComponents-2";
            var doc = XDocument.Parse(xmlContent);

            var datos = new DatosFacturaXml
            {
                NumeroCompleto = doc.Descendants(cbc + "ID").FirstOrDefault()?.Value,
                TipoDocumento = doc.Descendants(cbc + "InvoiceTypeCode").FirstOrDefault()?.Value,
                FechaEmision = doc.Descendants(cbc + "IssueDate").FirstOrDefault()?.Value,
                Moneda = doc.Descendants(cbc + "DocumentCurrencyCode").FirstOrDefault()?.Value ?? "PEN",
                MontoTotal = decimal.Parse(doc.Descendants(cbc + "PayableAmount").FirstOrDefault()?.Value ?? "0"),
                MontoBase = decimal.Parse(doc.Descendants(cbc + "LineExtensionAmount").FirstOrDefault()?.Value ?? "0"),
                MontoIgv = decimal.Parse(doc.Descendants(cbc + "TaxAmount").FirstOrDefault()?.Value ?? "0"),
                TipoCambio = 1.0000m,
                Detalles = new List<DetalleFacturaXml>()
            };

            // Tipo de operación
            var profileId = doc.Descendants(cbc + "ProfileID").FirstOrDefault()?.Value;
            datos.TipoOperacion = profileId ?? "0101";

            // Serie y número
            var numeroCompleto = datos.NumeroCompleto?.Split('-');
            if (numeroCompleto?.Length == 2)
            {
                datos.Serie = numeroCompleto[0];
                datos.Numero = numeroCompleto[1];
            }

            // Fecha vencimiento
            var fechaVenc = doc.Descendants(cbc + "DueDate").FirstOrDefault()?.Value;
            if (!string.IsNullOrEmpty(fechaVenc))
                datos.FechaVencimiento = fechaVenc;

            // Cliente
            var cliente = doc.Descendants(cac + "AccountingCustomerParty").FirstOrDefault();
            if (cliente != null)
            {
                var clienteId = cliente.Descendants(cbc + "ID").FirstOrDefault();
                datos.ClienteRuc = clienteId?.Value;
                datos.ClienteTipoDocumento = clienteId?.Attribute("schemeID")?.Value ?? "6";
                datos.ClienteRazonSocial = cliente.Descendants(cbc + "RegistrationName").FirstOrDefault()?.Value;
            }

            // Detalles
            var lineas = doc.Descendants(cac + "InvoiceLine");
            int numeroLinea = 1;
            foreach (var linea in lineas)
            {
                var detalle = new DetalleFacturaXml
                {
                    NumeroLinea = numeroLinea++,
                    Cantidad = decimal.Parse(linea.Element(cbc + "InvoicedQuantity")?.Value ?? "0"),
                    UnidadMedida = linea.Element(cbc + "InvoicedQuantity")?.Attribute("unitCode")?.Value,
                    ValorVenta = decimal.Parse(linea.Element(cbc + "LineExtensionAmount")?.Value ?? "0"),
                    PrecioUnitario = decimal.Parse(linea.Descendants(cbc + "PriceAmount").LastOrDefault()?.Value ?? "0"),
                    CodigoProducto = linea.Descendants(cbc + "ID").Skip(1).FirstOrDefault()?.Value,
                    Descripcion = linea.Descendants(cbc + "Description").FirstOrDefault()?.Value,
                    TipoAfectacionIgv = linea.Descendants(cbc + "TaxExemptionReasonCode").FirstOrDefault()?.Value ?? "10"
                };

                // IGV de línea
                var taxAmount = linea.Descendants(cbc + "TaxAmount").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(taxAmount))
                    detalle.MontoIgv = decimal.Parse(taxAmount);

                // Precio con IGV
                var precioConIgv = linea.Descendants(cac + "AlternativeConditionPrice")
                    .Descendants(cbc + "PriceAmount").FirstOrDefault()?.Value;
                if (!string.IsNullOrEmpty(precioConIgv))
                    detalle.PrecioUnitarioConIgv = decimal.Parse(precioConIgv);

                detalle.TotalLinea = detalle.ValorVenta + detalle.MontoIgv;
                datos.Detalles.Add(detalle);
            }

            return datos;
        }
    }
}
