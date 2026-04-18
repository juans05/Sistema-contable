using Microsoft.Extensions.Logging;
using ClosedXML.Excel;
using SistemaContable.Application.Services.Interfaces;
using SistemaContable.Application.Services.Interfaces.IRepository;
using SistemaContable.Domain.Entities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Implementations
{
    public class AccountingEngineService : IAccountingEngineService
    {
        private readonly IAccountingRepository _accountingRepo;
        private readonly IFacturaElectronicaRepository _ventaRepo;
        private readonly ICompraRepository _compraRepo;
        private readonly ILogger<AccountingEngineService> _logger;
        private readonly ITokenDataService _tokenDataService;

        public AccountingEngineService(
            IAccountingRepository accountingRepo,
            IFacturaElectronicaRepository ventaRepo,
            ICompraRepository compraRepo,
            ILogger<AccountingEngineService> logger,
            ITokenDataService tokenDataService)
        {            
            _accountingRepo = accountingRepo;
            _ventaRepo = ventaRepo;
            _compraRepo = compraRepo;
            _logger = logger;
            _tokenDataService = tokenDataService;
        }

        public async Task<int> GenerarAsientoVentaAsync(int registroVentaId, IUnitOfWork tx = null)
        {
            try
            {
                var accountingRepo = tx != null ? tx.AccountingRepo : _accountingRepo;
                var ventaRepo = tx != null ? tx.FacturaRepo : _ventaRepo;

                // 1. Obtener la Venta Completa
                 var venta = await ventaRepo.ObtenerVentaCompletaAsync(registroVentaId);
                if (venta == null)
                    throw new Exception($"Venta {registroVentaId} no encontrada");

                // TODO: Determinar el tipo de evento dinámicamente. 
                // Por ahora asumimos VENTA_MERCADERIA_CONTADO por defecto.
                // En el futuro, esto podría depender de "Forma de Pago" (Contado/Crédito).
                string codigoEvento = "VENTA_MERCADERIA_CONTADO";

                // FIXME: Obtener real empresaId
                int empresaId = _tokenDataService.GetEmpresaId();
                if (empresaId == 0)
                    throw new Exception("Empresa no identificada en el token");

                // Obtener configuración de decimales
                int decimalesMonto = 2; // Default
                string configDecimales = await accountingRepo.ObtenerConfiguracionAsync("DECIMALES_MONEDA_NACIONAL", empresaId);
                if (!string.IsNullOrEmpty(configDecimales) && int.TryParse(configDecimales, out int d))
                {
                    decimalesMonto = d;
                }

                // 2. Obtener Reglas Configurable
                var reglas = await accountingRepo.ObtenerReglasPorEventoAsync(codigoEvento, empresaId);

                if (reglas == null || !reglas.Any())
                {
                    _logger.LogWarning($"No hay reglas contables para el evento {codigoEvento}");
                    return 0;
                }

                // 3. Construir Cabecera del Asiento
                var asiento = new EAsientoContable
                {
                    EmpresaId = empresaId,
                    Periodo = venta.FechaEmision.ToString("yyyyMM"),
                    FechaContable = venta.FechaEmision.Date,
                    Glosa = $"Venta {venta.NumeroDocumento} - {venta.RazonSocialCliente}",
                    OrigenModulo = "VENTAS",
                    OrigenIdReferencia = venta.IdRegVenta,
                    CodigoUnicoOperacion = Guid.NewGuid().ToString().Substring(0, 8), // Temporal CUO
                    Moneda = venta.Moneda,
                    TipoCambio = 3.80m, // FIXME: Obtener TC del día
                    Estado = "MAYORIZADO",
                    UsuarioCreacion = "MOTOR_CONTABLE"
                };

                // 4. Procesar Reglas -> Detalles
                foreach (var regla in reglas)
                {
                    decimal montoCalculado = 0;

                    // Evaluar Fórmula
                    switch (regla.FormulaMonto)
                    {
                        case "TOTAL":
                            montoCalculado = venta.TotalDoc;
                            break;
                        case "BASE_IMPONIBLE":
                            montoCalculado = venta.SubTotal;
                            break;
                        case "IGV":
                            montoCalculado = venta.ImpIgv;
                            break;
                        default:
                            montoCalculado = 0;
                            break;
                    }

                    if (montoCalculado == 0) continue; 
                    
                    // APLICAR REDONDEO CONFIGURABLE
                    montoCalculado = Math.Round(montoCalculado, decimalesMonto);

                    // Resolver Cuenta
                    string cuentaFinal = regla.CuentaCodigoBase;

                    if (!string.IsNullOrEmpty(regla.CuentaDinamicaTipo))
                    {
                        // Lógica para cuentas dinámicas (Ej: Cuenta del Cliente específico)
                        // Por ahora no implementado, usa la base.
                    }
                    
                    // Buscar descripción de la cuenta para guardarla en el detalle (snapshot)
                    var planCuenta = await accountingRepo.ObtenerCuentaPorCodigoAsync(cuentaFinal, empresaId);
                    string descripcionCuenta = planCuenta?.Nombre ?? "CUENTA DESCONOCIDA";

                    var linea = new EAsientoContableDetalle
                    {
                        CuentaCodigo = cuentaFinal,
                        DescripcionCuenta = descripcionCuenta,
                        Orden = regla.Orden
                    };

                    // Asignar Debe/Haber
                    if (regla.Naturaleza == "D")
                    {
                        linea.DebeOrigen = montoCalculado;
                        linea.HaberOrigen = 0;
                        if (venta.Moneda == "PEN") { linea.DebePen = montoCalculado; }
                        else { linea.DebeUsd = montoCalculado; }
                    }
                    else
                    {
                        linea.DebeOrigen = 0;
                        linea.HaberOrigen = montoCalculado;
                         if (venta.Moneda == "PEN") { linea.HaberPen = montoCalculado; }
                        else { linea.HaberUsd = montoCalculado; }
                    }

                    asiento.Detalles.Add(linea);
                }

                // Validar Partida Doble (Opcional, pero recomendado)
                decimal totalDebe = asiento.Detalles.Sum(d => d.DebeOrigen);
                decimal totalHaber = asiento.Detalles.Sum(d => d.HaberOrigen);

                if (Math.Abs(totalDebe - totalHaber) > (decimal)Math.Pow(10, -decimalesMonto))
                {
                    _logger.LogError($"Asiento descuadrado: Debe {totalDebe} vs Haber {totalHaber}");
                     // Podríamos guardar como BORRADOR o lanzar error. 
                     // Para MVP lanzamos error.
                     throw new InvalidOperationException($"El asiento generado no cuadra (Partida Doble). Diferencia: {totalDebe - totalHaber}");
                }

                // 5. Guardar Asiento
                return await accountingRepo.GuardarAsientoCompletoAsync(asiento);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando asiento automático para venta {Id}", registroVentaId);
                throw;
            }
        }
        public async Task<int> GenerarAsientoCompraAsync(int registroCompraId, IUnitOfWork tx = null)
        {
            try
            {
                var accountingRepo = tx != null ? tx.AccountingRepo : _accountingRepo;
                var compraRepo = tx != null ? tx.CompraRepo : _compraRepo;

                // 1. Obtener Compra
                var compra = await compraRepo.ObtenerCompraPorIdAsync(registroCompraId);

                if (compra == null) throw new Exception($"Compra {registroCompraId} no encontrada");

                // Configuración
                int empresaId = _tokenDataService.GetEmpresaId();
                if (empresaId == 0)
                    throw new Exception("Empresa no identificada en el token");
                string codigoEvento = "COMPRA_MERCADERIA"; 

                // Obtener decimales
                int decimalesMonto = 2; 
                string configDecimales = await accountingRepo.ObtenerConfiguracionAsync("DECIMALES_MONEDA_NACIONAL", empresaId);
                if (!string.IsNullOrEmpty(configDecimales) && int.TryParse(configDecimales, out int d)) decimalesMonto = d;


                try
                {

                    // 2. Obtener Reglas
                    var reglas = await accountingRepo.ObtenerReglasPorEventoAsync(codigoEvento, empresaId);
                    if (reglas == null || !reglas.Any())
                    {
                        return 0;
                    }

                    // 3. Cabecera Asiento
                    var asiento = new EAsientoContable
                    {
                        EmpresaId = empresaId,
                        Periodo = compra.FEmisc.ToString("yyyyMM"),
                        FechaContable = compra.FEmisc,
                        Glosa = $"Compra {compra.SerieDocumento}-{compra.NoDocumento} - {compra.NombreProv}",
                        OrigenModulo = "COMPRAS",
                        OrigenIdReferencia = compra.IdRegCompras,
                        CodigoUnicoOperacion = Guid.NewGuid().ToString().Substring(0, 8),
                        Moneda = compra.Moneda,
                        TipoCambio = 3.80m, // FIXME
                        Estado = "MAYORIZADO",
                        UsuarioCreacion = "MOTOR_CONTABLE"
                    };

                    // 4. Procesar Reglas
                    foreach (var regla in reglas)
                    {
                        decimal montoCalculado = 0;
                        switch (regla.FormulaMonto)
                        {
                            case "TOTAL": montoCalculado = compra.TotalDoc; break;
                            case "BASE_IMPONIBLE": montoCalculado = compra.SubTotal; break;
                            case "IGV": montoCalculado = compra.ImpIgv; break;
                            default: montoCalculado = 0; break;
                        }

                        if (montoCalculado == 0) continue;
                        montoCalculado = Math.Round(montoCalculado, decimalesMonto);

                        var planCuenta = await accountingRepo.ObtenerCuentaPorCodigoAsync(regla.CuentaCodigoBase, empresaId);

                        var linea = new EAsientoContableDetalle
                        {
                            CuentaCodigo = regla.CuentaCodigoBase,
                            DescripcionCuenta = planCuenta?.Nombre ?? "DESC",
                            Orden = regla.Orden
                        };

                        if (regla.Naturaleza == "D")
                        {
                            linea.DebeOrigen = montoCalculado;
                            if (compra.Moneda == "PEN") linea.DebePen = montoCalculado;
                            else linea.DebeUsd = montoCalculado;
                        }
                        else
                        {
                            linea.HaberOrigen = montoCalculado;
                            if (compra.Moneda == "PEN") linea.HaberPen = montoCalculado;
                            else linea.HaberUsd = montoCalculado;
                        }
                        asiento.Detalles.Add(linea);
                    }

                    // 5. Validar y Guardar (Provisión)
                    int asientoId = await accountingRepo.GuardarAsientoCompletoAsync(asiento);

                    // 6. Generar Asiento de DESTINO (20 vs 61) si aplica
                    await GenerarDestinoAutomatico(compra, empresaId, decimalesMonto, accountingRepo);


                    return asientoId;
                }
                catch (Exception exTransaction)
                {
                    _logger.LogError(exTransaction, "Error durante la transacción del asiento compra {Id}", registroCompraId);
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generando asiento compra {Id}", registroCompraId);
                throw;
            }
        }

        private async Task GenerarDestinoAutomatico(ERegistroCompra compra, int empresaId, int decimales, IAccountingRepository txRepo)
        {
            try 
            {
                string codigoEvento = "DESTINO_MERCADERIA";
                var reglas = await txRepo.ObtenerReglasPorEventoAsync(codigoEvento, empresaId);
                if (reglas == null || !reglas.Any()) return;

                var asiento = new EAsientoContable
                {
                    EmpresaId = empresaId,
                    Periodo = compra.FEmisc.ToString("yyyyMM"),
                    FechaContable = compra.FEmisc,
                    Glosa = $"Destino (Almacén) {compra.SerieDocumento}-{compra.NoDocumento}",
                    OrigenModulo = "LOGISTICA",
                    OrigenIdReferencia = compra.IdRegCompras,
                    CodigoUnicoOperacion = Guid.NewGuid().ToString().Substring(0, 8),
                    Moneda = compra.Moneda,
                    TipoCambio = 3.80m,
                    Estado = "MAYORIZADO",
                    UsuarioCreacion = "MOTOR_CONTABLE"
                };

                foreach (var regla in reglas)
                {
                    decimal monto = (regla.FormulaMonto == "BASE_IMPONIBLE") ? compra.SubTotal : 0; // Usualmente costo
                    if (monto == 0) continue;
                    monto = Math.Round(monto, decimales);

                    var plan = await txRepo.ObtenerCuentaPorCodigoAsync(regla.CuentaCodigoBase, empresaId);

                    var linea = new EAsientoContableDetalle
                    {
                        CuentaCodigo = regla.CuentaCodigoBase,
                        DescripcionCuenta = plan?.Nombre??string.Empty,
                        Orden = regla.Orden
                    };

                    if (regla.Naturaleza == "D") {
                        linea.DebeOrigen = monto;
                        if (compra.Moneda == "PEN") linea.DebePen = monto; else linea.DebeUsd = monto;
                    } else {
                        linea.HaberOrigen = monto;
                        if (compra.Moneda == "PEN") linea.HaberPen = monto; else linea.HaberUsd = monto;
                    }
                    asiento.Detalles.Add(linea);
                }

                await txRepo.GuardarAsientoCompletoAsync(asiento);
            }

            catch(Exception ex)
            {
                _logger.LogError(ex, "Error destino compra");
                throw; // Lanza error para abortar la transacción completa
            }
        }

        public async Task<bool> ImportarPlanCuentasExcelAsync(System.IO.Stream excelStream, int empresaId)
        {
            try
            {
                var cuentas = new System.Collections.Generic.List<EPlanContable>();
                
                using (var workbook = new ClosedXML.Excel.XLWorkbook(excelStream))
                {
                    var worksheet = workbook.Worksheet(1); // Primer hoja
                    var rows = worksheet.RangeUsed().RowsUsed().Skip(1); // Saltar encabezados

                    foreach (var row in rows)
                    {
                        var codigo = row.Cell(1).GetString();
                        if (string.IsNullOrWhiteSpace(codigo)) continue;

                        var cuenta = new EPlanContable
                        {
                            Codigo = codigo,
                            Nombre = row.Cell(2).GetString(),
                            Nivel = int.TryParse(row.Cell(3).GetValue<string>(), out int n) ? n : 0,
                            TipoCuenta = row.Cell(4).GetString(),
                            Moneda = row.Cell(5).GetString(), // PEN/USD
                            Analisis = row.Cell(6).GetString(),
                            PermiteMovimiento = row.Cell(7).GetValue<string>()?.ToUpper() == "SI" || row.Cell(7).GetValue<string>()?.ToUpper() == "TRUE",
                            Activo = true,
                            EmpresaId = empresaId
                        };
                        cuentas.Add(cuenta);
                    }
                }

                if (!cuentas.Any()) throw new Exception("El archivo Excel no contiene cuentas válidas.");

                return await _accountingRepo.ImportarPlanCuentasAsync(cuentas, empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando Excel de Plan de Cuentas");
                throw;
            }
        }
    }
}
