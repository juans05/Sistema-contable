using System;

namespace SistemaContable.Domain.Entities
{
    public class EReglaContable
    {
        public int Id { get; set; }
        public int EventoTipoId { get; set; }
        public int Orden { get; set; }
        
        // Selección de Cuenta
        public string CuentaCodigoBase { get; set; }
        public string CuentaDinamicaTipo { get; set; }
        
        // Definición del Monto
        public string Naturaleza { get; set; } // D, H
        public string FormulaMonto { get; set; } // TOTAL, IGV, ETC.
        
        public string GlosaPlantilla { get; set; }
        public string CondicionSql { get; set; }
        
        public int? EmpresaId { get; set; }
        public bool Activo { get; set; }
    }
}
