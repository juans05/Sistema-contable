namespace SistemaContable.Domain.Models
{
    public class PlanContableDetalleDto
    {
        public int Id { get; set; }
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string? Elemento { get; set; }
        public string? Cta { get; set; }
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
        
        public string? CuentaCierre { get; set; }
        public bool Activo { get; set; }
    }

    public class CrearPlanContableRequest
    {
        public string Cuenta { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string? Elemento { get; set; }
        public string? Cta { get; set; }
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
        
        public string? CuentaCierre { get; set; }
        public bool Activo { get; set; } = true;
    }

    public class ActualizarPlanContableRequest : CrearPlanContableRequest
    {
    }
}