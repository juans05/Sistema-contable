using System;

namespace SistemaContable.Domain.Entities
{
    public class EPlanContableGeneral
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string? Elemento { get; set; }
        public string? Cta { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string? ClaseCuenta { get; set; }
        public string? TipoAnexo { get; set; }
        public bool CuentaMonetaria { get; set; }
        public bool AjusteDifCambio { get; set; }
        public bool RequiereCentroCosto { get; set; }
        public string? CodigoEeffEstand { get; set; }
        public string? CodigoEeffTrib { get; set; }
        public string? ClasificacionBienServ { get; set; }
        public string? Cargo1 { get; set; }
        public string? Abono1 { get; set; }
        public decimal Porcentaje1 { get; set; }
        public string? Cargo2 { get; set; }
        public string? Abono2 { get; set; }
        public decimal Porcentaje2 { get; set; }
        public string? Cargo3 { get; set; }
        public string? Abono3 { get; set; }
        public decimal Porcentaje3 { get; set; }
        public string? CuentaCierre { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}