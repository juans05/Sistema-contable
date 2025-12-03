using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class EEmpresa
    {
        public Guid Id { get; set; }
        public string razon_social { get; set; } = string.Empty;
        public string nombre_comercial { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? regimen_tributario { get; set; }
        public string? tipo_contribuyente { get; set; }
        public DateTime? FechaConstitucion { get; set; }
        public string? representante_legal { get; set; }
        public string? dni_representante { get; set; }
        public string? logo_url { get; set; }
        public bool activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public ICollection<EEmpresaUsuario> EmpresaUsuarios { get; set; } = new List<EEmpresaUsuario>();
    }
}
