using Dapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using SistemaContable.Application.DTOs.Dashboard;
using SistemaContable.Application.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace SistemaContable.Application.Services.Implementations
{
    public class FinancialDashboardService : IFinancialDashboardService
    {
        private readonly string _connectionString;
        private readonly ILogger<FinancialDashboardService> _logger;

        public FinancialDashboardService(IConfiguration configuration, ILogger<FinancialDashboardService> logger)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                 ?? throw new InvalidOperationException("Connection string no configurada");
            _logger = logger;
        }

        public async Task<FinancialSummaryDto> ObtenerResumenMensualAsync(string rucEmpresa, int mes, int anio)
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                // 1. Ventas Netas (Clase 70 - Haber/Crédito)
                // Usamos ABS para tener valor positivo, aunque Haber suele guardarse positivo o negativo según diseño, 
                // asumiremos CreditAmount como positivo.
                var sqlVentas = @"
                    SELECT SUM(d.credit_amount) 
                    FROM ""suizaConta"".accounting_entries e
                    JOIN ""suizaConta"".accounting_entry_details d ON e.id = d.accounting_entry_id
                    WHERE e.ruc_empresa = @Ruc 
                      AND EXTRACT(MONTH FROM e.entry_date) = @Mes 
                      AND EXTRACT(YEAR FROM e.entry_date) = @Anio
                      AND d.account_code LIKE '70%'
                      AND e.status = 'POSTED'";

                // 2. Gastos Operativos (Clase 60, 63, 64, 65 - Debe/Débito)
                var sqlGastos = @"
                    SELECT SUM(d.debit_amount) 
                    FROM ""suizaConta"".accounting_entries e
                    JOIN ""suizaConta"".accounting_entry_details d ON e.id = d.accounting_entry_id
                    WHERE e.ruc_empresa = @Ruc 
                      AND EXTRACT(MONTH FROM e.entry_date) = @Mes 
                      AND EXTRACT(YEAR FROM e.entry_date) = @Anio
                      AND (d.account_code LIKE '60%' OR d.account_code LIKE '63%' OR d.account_code LIKE '64%' OR d.account_code LIKE '65%')
                      AND e.status = 'POSTED'";

                // 3. IGV Ventas (40111 - Haber/Crédito)
                var sqlIgvVentas = @"
                    SELECT SUM(d.credit_amount) 
                    FROM ""suizaConta"".accounting_entries e
                    JOIN ""suizaConta"".accounting_entry_details d ON e.id = d.accounting_entry_id
                    WHERE e.ruc_empresa = @Ruc 
                      AND EXTRACT(MONTH FROM e.entry_date) = @Mes 
                      AND EXTRACT(YEAR FROM e.entry_date) = @Anio
                      AND d.account_code = '40111'
                      AND e.status = 'POSTED'";

                // 4. IGV Compras (40111 - Debe/Débito)
                var sqlIgvCompras = @"
                     SELECT SUM(d.debit_amount) 
                    FROM ""suizaConta"".accounting_entries e
                    JOIN ""suizaConta"".accounting_entry_details d ON e.id = d.accounting_entry_id
                    WHERE e.ruc_empresa = @Ruc 
                      AND EXTRACT(MONTH FROM e.entry_date) = @Mes 
                      AND EXTRACT(YEAR FROM e.entry_date) = @Anio
                      AND d.account_code = '40111'
                      AND e.status = 'POSTED'";

                var ventas = await connection.ExecuteScalarAsync<decimal?>(sqlVentas, new { Ruc = rucEmpresa, Mes = mes, Anio = anio }) ?? 0;
                var gastos = await connection.ExecuteScalarAsync<decimal?>(sqlGastos, new { Ruc = rucEmpresa, Mes = mes, Anio = anio }) ?? 0;
                var igvVentas = await connection.ExecuteScalarAsync<decimal?>(sqlIgvVentas, new { Ruc = rucEmpresa, Mes = mes, Anio = anio }) ?? 0;
                var igvCompras = await connection.ExecuteScalarAsync<decimal?>(sqlIgvCompras, new { Ruc = rucEmpresa, Mes = mes, Anio = anio }) ?? 0;

                return new FinancialSummaryDto
                {
                    VentasNetas = ventas,
                    GastosOperativos = gastos,
                    IgvVentas = igvVentas,
                    IgvCompras = igvCompras,
                    Periodo = $"{mes:00}-{anio}"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo dashboard financiero");
                return new FinancialSummaryDto();
            }
        }
    }
}
