using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Threading.Tasks;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;

namespace SistemaContable.Application.Services.Implementations
{
    public class SireService : ISireService
    {
        private readonly IFacturaElectronicaRepository _facturaRepo;
        private readonly ICompraRepository _compraRepo;

        public SireService(IFacturaElectronicaRepository facturaRepo, ICompraRepository compraRepo)
        {
            _facturaRepo = facturaRepo;
            _compraRepo = compraRepo;
        }

        public async Task<byte[]> GenerarRvieReemplazoAsync(string periodo, string rucEmpresa)
        {
            // 1. Obtener datos (YYYYMM)
            var ventas = await _facturaRepo.ListarVentasParaSire(periodo, rucEmpresa);

            // 2. Construir contenido TXT (Pipe separated)
            var sb = new StringBuilder();
            
            // Periodo formato: 20240100
            // Nombre archivo: LE + RUC + AAAA + MM + 00 + 140400 + 02 + 1 + 1 + 1 + 2 + .txt
            // Estructura RVIE (Anexo 3 - Simplificado MVP)
            foreach (var v in ventas)
            {
                // Col 1: RUC Emisor
                sb.Append(v.RucEmpresa).Append("|");
                // Col 2: Razón Social
                sb.Append(v.RazonSocialEmpresa).Append("|");
                // Col 3: Periodo (YYYYMM00)
                sb.Append(v.Periodo + "00").Append("|");
                // Col 4: CAR (Código Anotación) - Dejamos vacío o hardcode si es necesario
                sb.Append("|"); 
                // Col 5: Fecha Emisión (DP)
                sb.Append(v.FechaEmision.ToString("td/MM/yyyy")).Append("|");
                // Col 6: Fecha Vto (DP)
                sb.Append(v.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "").Append("|");
                // Col 7: Tipo Comprobante
                sb.Append(v.TipoComprobante).Append("|");
                // Col 8: Serie
                sb.Append(v.Serie).Append("|");
                // Col 9: Numero
                sb.Append(v.Numero).Append("|");
                // Col 10: Importe Total Final (se usa para algunos recalculos) -> OJO, estructura varía.
                
                // AJUSTE: La estructura RVIE tiene >30 columnas. Haremos un mapping "Best Effort" para MVP.
                // Col 10: Tipo Doc Cliente
                sb.Append(v.TipoDocCliente).Append("|");
                // Col 11: RUC Cliente
                sb.Append(v.RucCliente).Append("|");
                // Col 12: Razón Social Cliente
                sb.Append(v.RazonSocialCliente).Append("|");
                // Col 13: Valor Facturado Exportación
                sb.Append(FormatAmount(v.ValoFacturadoExportacion)).Append("|");
                // Col 14: Base Imponible Gravada
                sb.Append(FormatAmount(v.BaseImponibleGravada)).Append("|");
                // Col 15: Descuento BI (0.00)
                sb.Append("0.00|");
                // Col 16: IGV
                sb.Append(FormatAmount(v.MontoIgv)).Append("|");
                // Col 17: Exonerado
                sb.Append(FormatAmount(v.MontoExonerado)).Append("|");
                // Col 18: Inafecto
                sb.Append(FormatAmount(v.MontoInafecto)).Append("|");
                // Col 19: ISC
                sb.Append(FormatAmount(v.MontoIsc)).Append("|");
                // Col 20: Base Arroz (IVAP)
                sb.Append("0.00|");
                // Col 21: IVAP
                sb.Append("0.00|");
                // Col 22: ICBPER
                sb.Append(FormatAmount(v.MontoIcbper)).Append("|");
                // Col 23: Otros Tributos
                sb.Append("0.00|");
                // Col 24: Total Comprobante
                sb.Append(FormatAmount(v.TotalComprobante)).Append("|");
                // Col 25: Moneda
                sb.Append(v.Moneda).Append("|");
                // Col 26: Tipo Cambio
                sb.Append(v.Moneda == "PEN" ? "" : v.TipoCambio.ToString("F3")).Append("|");

                // Referencias (NC/ND) - Col 27-30
                if (v.TipoComprobante == "07" || v.TipoComprobante == "08")
                {
                    sb.Append(v.FechaReferencia?.ToString("dd/MM/yyyy") ?? "").Append("|");
                    sb.Append(v.TipoReferencia ?? "").Append("|");
                    sb.Append(v.SerieReferencia ?? "").Append("|");
                    sb.Append(v.NumeroReferencia ?? "").Append("|");
                }
                else
                {
                    sb.Append("||||");
                }
                
                // Col 31, 32, 33... (Datos extra, id proyecto, etc)
                sb.Append("||||"); 
                // Col 36: Estado (1: Activo)
                sb.Append("1|"); 
                
                sb.AppendLine();
            }

            var txtBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // 3. Generar ZIP
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entryName = $"LE{rucEmpresa}{periodo}00140400021112.txt"; 
                // Nombre Estándar: LE + RUC + AAAA + MM + 00 + 140400 + 02 + 1 + 1 + 1 + 2
                // 140400 = RVIE
                // 02 = Oportunidad (Reemplazo Propuesta)
                
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(txtBytes, 0, txtBytes.Length);
            }
            
            return memoryStream.ToArray();
        }

        public async Task<byte[]> GenerarRceReemplazoAsync(string periodo, string rucEmpresa)
        {
            var compras = await _compraRepo.ListarComprasParaSire(periodo, rucEmpresa);
            var sb = new StringBuilder();

            // Estructura RCE (Simplificada - Reemplazo 8.4 / Anexo 11)
            // Filename: LE + RUC + AAAA + MM + 00 + 080400 + 02 + 1 + 1 + 1 + 2 + .txt
            foreach (var c in compras)
            {
                // 1. RUC (Emisor)
                sb.Append(rucEmpresa).Append("|");
                // 2. Razon Social (Emisor)
                sb.Append(c.RazonSocialEmpresa).Append("|");
                // 3. Periodo
                sb.Append(c.Periodo + "00").Append("|");
                // 4. CAR
                sb.Append(c.Car ?? "").Append("|");
                // 5. Fecha Emision
                sb.Append(c.FechaEmision.ToString("dd/MM/yyyy")).Append("|");
                // 6. Fecha Vto
                sb.Append(c.FechaVencimiento?.ToString("dd/MM/yyyy") ?? "").Append("|");
                // 7. Tipo Comprobante
                sb.Append(c.TipoComprobante).Append("|");
                // 8. Serie
                sb.Append(c.Serie).Append("|");
                // 9. Año Emision DUA
                sb.Append(c.AnioEmisionDua ?? "").Append("|");
                // 10. Numero
                sb.Append(c.Numero).Append("|");
                // 11. Numero Final (Rango)
                sb.Append("|");
                // 12. Tipo Doc Proveedor
                sb.Append(c.TipoDocProveedor).Append("|");
                // 13. RUC Proveedor
                sb.Append(c.RucProveedor).Append("|");
                // 14. Razon Social Proveedor
                sb.Append(c.RazonSocialProveedor).Append("|");
                
                // 15. BI Gravada DG
                sb.Append(FormatAmount(c.BaseImponibleGravadaDG)).Append("|");
                // 16. IGV DG
                sb.Append(FormatAmount(c.IgvDG)).Append("|");
                
                // 17. BI Gravada DM
                sb.Append(FormatAmount(c.BaseImponibleGravadaDM)).Append("|");
                // 18. IGV DM
                sb.Append(FormatAmount(c.IgvDM)).Append("|");
                
                // 19. BI Gravada DNG
                sb.Append(FormatAmount(c.BaseImponibleGravadaDNG)).Append("|");
                // 20. IGV DNG
                sb.Append(FormatAmount(c.IgvDNG)).Append("|");
                
                // 21. No Gravadas (Exonerado / Inafecto)
                sb.Append(FormatAmount(c.MontoExonerado + c.MontoInafecto)).Append("|");
                
                // 22. ISC
                sb.Append(FormatAmount(c.MontoIsc)).Append("|");
                // 23. ICBPER
                sb.Append(FormatAmount(c.MontoIcbper)).Append("|");
                // 24. Otros
                sb.Append(FormatAmount(c.OtrosTributos)).Append("|");
                // 25. Total
                sb.Append(FormatAmount(c.TotalComprobante)).Append("|");
                
                // 26. Moneda
                sb.Append(c.Moneda).Append("|");
                // 27. Tipo Cambio
                sb.Append(c.Moneda == "PEN" ? "" : c.TipoCambio.ToString("F3")).Append("|");
                
                // 28-31. Referencias NC/ND
                if (c.TipoComprobante == "07" || c.TipoComprobante == "08")
                {
                    sb.Append(c.FechaReferencia?.ToString("dd/MM/yyyy") ?? "").Append("|");
                    sb.Append(c.TipoReferencia ?? "").Append("|");
                    sb.Append(c.SerieReferencia ?? "").Append("|");
                    sb.Append(c.NumeroReferencia ?? "").Append("|");
                }
                else
                {
                    sb.Append("||||");
                }
                
                // ... Otros campos (Detracciones, etc) dejamos vacíos por simplicidad MVP
                sb.Append("|||||||||||||");
                
                // Estado 1 (Activo)
                sb.Append("1|");
                
                sb.AppendLine();
            }

             var txtBytes = Encoding.UTF8.GetBytes(sb.ToString());

            // ZIP
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                var entryName = $"LE{rucEmpresa}{periodo}00080400021112.txt"; 
                // 080400 = RCE
                // 02 = Reemplazo
                
                var entry = archive.CreateEntry(entryName);
                using var entryStream = entry.Open();
                await entryStream.WriteAsync(txtBytes, 0, txtBytes.Length);
            }
            
            return memoryStream.ToArray();
        }

        private string FormatAmount(decimal amount)
        {
            return amount.ToString("F2");
        }
    }
}
