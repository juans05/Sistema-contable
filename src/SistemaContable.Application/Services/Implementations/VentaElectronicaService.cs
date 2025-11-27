using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SistemaContable.Application.DTOs;
using SistemaContable.Application.DTOs.Common;
using SistemaContable.Application.DTOs.Responses.XML;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SistemaContable.Application.Services.Implementations
{
    public class VentaElectronicaService : IVentaElectronicaService
    {
        private readonly IVentaRepository _ventaRepository;
        private readonly IFacturaElectronicaRepository _facturaRepository;
        private readonly ILogger<VentaElectronicaService> _logger;
        public VentaElectronicaService(
            IVentaRepository ventaRepository,
            IFacturaElectronicaRepository facturaRepository,
            ILogger<VentaElectronicaService> logger)
        {
            _ventaRepository = ventaRepository;
            _facturaRepository = facturaRepository;
            _logger = logger;
        }
        public async Task<ProcesarXmlVentaResponseDto> ProcesarXmlYRegistrarVentaAsync(List<IFormFile> archivosXml, string usuario)
        {
            var response = new ProcesarXmlVentaResponseDto
            {
                Resultados = new List<ResultadoProcesamiento>()
            };

            foreach (var archivo in archivosXml)
            {
                var resultado = new ResultadoProcesamiento
                {
                    NombreArchivo = archivo.FileName
                };

                try
                {
                    // 1. Leer XML
                    using var stream = archivo.OpenReadStream();
                    using var reader = new StreamReader(stream);
                    var xmlContent = await reader.ReadToEndAsync();

                    // 2. Parsear XML
                    var datosXml = ParsearFacturaXml(xmlContent);

                    // 3. Verificar duplicados
                    var hash = CalcularHash(xmlContent);
                    if (await _facturaRepository.ExisteFacturaPorHashAsync(hash))
                    {
                        resultado.Procesado = false;
                        resultado.Error = "Documento ya registrado (duplicado)";
                        response.Resultados.Add(resultado);
                        continue;
                    }

                    // 4. Guardar Factura Electrónica
                    var factura = new EFacturaElectronica
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
                        EstadoSunat = "PENDIENTE",
                        CodigoHash = hash,
                        XmlOriginal = xmlContent,
                        CreatedAt = DateTime.UtcNow
                    };

                    var facturaGuardada = await _facturaRepository.CrearAsync(factura);

                    // 5. Crear Registro de Venta
                    var venta = new ERegistroVenta
                    {
                        IdFacturaElectronica = facturaGuardada.IdFacturaElectronica,
                        RucCliente = datosXml.ClienteRuc,
                        Periodo = datosXml.FechaEmision.ToString("yyyyMM"),
                        RSCliente = datosXml.ClienteRazonSocial,
                        TipoDoc = datosXml.TipoDocumento,
                        SerieDoc = datosXml.Serie,
                        NumDoc = datosXml.Numero,
                        FechaEmision = datosXml.FechaEmision,
                        FechaVencimiento = datosXml.FechaVencimiento,
                        TipCambio = datosXml.TipoCambio,
                        TipoDocCliente = datosXml.ClienteTipoDocumento,
                        Moneda = datosXml.Moneda,
                        SubTotal = datosXml.MontoBase,
                        ImpIgv = datosXml.MontoIgv,
                        TotalDoc = datosXml.MontoTotal,
                        EstadoDoc = "ACTIVO",
                        TipOperaSunat = datosXml.TipoOperacion,
                        CreatedAt = DateTime.UtcNow,
                        Detalles = new List<ERegistroVentaDetalle>()
                    };

                    // 6. Agregar detalles
                    foreach (var det in datosXml.Detalles)
                    {
                        venta.Detalles.Add(new ERegistroVentaDetalle
                        {
                            NumeroLinea = det.NumeroLinea,
                            CodigoProducto = det.CodigoProducto,
                            DescripcionProducto = det.Descripcion,
                            UnidadMedida = det.UnidadMedida,
                            Cantidad = det.Cantidad,
                            PrecioUnitario = det.PrecioUnitario,
                            PrecioUnitarioConIgv = det.PrecioUnitarioConIgv,
                            ValorVenta = det.ValorVenta,
                            MontoIgv = det.MontoIgv,
                            TotalLinea = det.TotalLinea,
                            TipoAfectacionIgv = det.TipoAfectacionIgv,
                            PorcentajeIgv = 18.00m
                        });
                    }

                    // 7. Guardar venta con detalles
                    var ventaGuardada = await _ventaRepository.CrearConDetallesAsync(venta);

                    resultado.Procesado = true;
                    resultado.NumeroDocumento = datosXml.NumeroCompleto;
                    resultado.IdVenta = ventaGuardada.IdRegVenta;
                    resultado.IdFacturaElectronica = facturaGuardada.IdFacturaElectronica;
                    response.VentasRegistradas++;

                    _logger.LogInformation(
                        "Venta registrada: {NumeroDocumento}, ID: {IdVenta}",
                        datosXml.NumeroCompleto, ventaGuardada.IdRegVenta
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error procesando archivo {Archivo}", archivo.FileName);
                    resultado.Procesado = false;
                    resultado.Error = ex.Message;
                }

                response.Resultados.Add(resultado);
            }

            response.Exito = response.VentasRegistradas > 0;
            response.Mensaje = $"Se procesaron {response.VentasRegistradas} de {archivosXml.Count} documentos";

            return response;
        }

        private string CalcularHash(string xmlContent)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(xmlContent));
            return Convert.ToBase64String(bytes);
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
                FechaEmision = DateTime.Parse(doc.Descendants(cbc + "Issue              Date").FirstOrDefault()?.Value),
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
                datos.FechaVencimiento = DateTime.Parse(fechaVenc);

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
