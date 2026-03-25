using System;

namespace SistemaContable.Domain.Entities
{
    public class EPlanContable
    {
        public int Id { get; set; }
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public int Nivel { get; set; }
        public string TipoCuenta { get; set; } // ACTIVO, PASIVO, PATRIMONIO, INGRESO, GASTO
        public string Moneda { get; set; }
        public string? Analisis { get; set; } // BANCOS, CLIENTES, etc.
        public bool PermiteMovimiento { get; set; }
        public bool Activo { get; set; }
        public int? EmpresaId { get; set; }
    }
}
