namespace SistemaContable.Application.DTOs.Dashboard
{
    public class FinancialSummaryDto
    {
        public decimal VentasNetas { get; set; }
        public decimal GastosOperativos { get; set; }
        public decimal UtilidadOperativa => VentasNetas - GastosOperativos;
        public decimal IgvVentas { get; set; }
        public decimal IgvCompras { get; set; }
        public decimal IgvPorPagarEstimado => IgvVentas - IgvCompras;
        public string Periodo { get; set; } // "MM-YYYY"
    }
}
