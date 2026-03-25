using System;
using System.Collections.Generic;

namespace SistemaContable.Domain.Entities
{
    public class EAsientoContable
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public string Periodo { get; set; }
        public DateTime FechaContable { get; set; }
        public string Glosa { get; set; }
        
        public string OrigenModulo { get; set; }
        public int? OrigenIdReferencia { get; set; }
        public string CodigoUnicoOperacion { get; set; }
        
        public string Moneda { get; set; }
        public decimal TipoCambio { get; set; }
        
        public string Estado { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string UsuarioCreacion { get; set; }

        // Propiedad de navegación (no mapeada directamente en Dapper simple, pero útil para lógica)
        public List<EAsientoContableDetalle> Detalles { get; set; } = new List<EAsientoContableDetalle>();
    }

    public class EAsientoContableDetalle
    {
        public int Id { get; set; }
        public int AsientoId { get; set; }
        public string CuentaCodigo { get; set; }
        public string DescripcionCuenta { get; set; }
        
        public decimal DebeOrigen { get; set; }
        public decimal HaberOrigen { get; set; }
        
        public decimal DebePen { get; set; }
        public decimal HaberPen { get; set; }
        
        public decimal DebeUsd { get; set; }
        public decimal HaberUsd { get; set; }
        
        public int Orden { get; set; }
    }
}
