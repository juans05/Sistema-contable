using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class EEmpresa
    {
        public Guid id { get; set; }
        public string razonSocial { get; set; } = string.Empty;
        public string nombreComercial { get; set; } = string.Empty;
        public string ruc { get; set; } = string.Empty;
        public string? direccion { get; set; }
        public string? telefono { get; set; }
        public string? email { get; set; }
        public string? web { get; set; }
        public string? regimenTributario { get; set; }
        public string? tipoContribuyente { get; set; }
        public DateTime? fechaConstitucion { get; set; }
        public string? representanteLegal { get; set; }
        public string? dniRepresentante { get; set; }
        public string? logoUrl { get; set; }
        public bool activo { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime? updatedAt { get; set; }
        public ICollection<EEmpresaUsuario> empresaUsuarios { get; set; } = new List<EEmpresaUsuario>();
    }
}
