using SistemaContable.Application.DTOs.Responses.Contador;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Responses.Empresa
{
    public class EmpresaResponse
    {
        public Guid Id { get; set; }
        public string RazonSocial { get; set; } = string.Empty;
        public string NombreComercial { get; set; } = string.Empty;
        public string Ruc { get; set; } = string.Empty;
        public string? Direccion { get; set; }
        public string? Telefono { get; set; }
        public string? Email { get; set; }
        public string? Web { get; set; }
        public string? RegimenTributario { get; set; }
        public string? TipoContribuyente { get; set; }
        public DateTime? FechaConstitucion { get; set; }
        public string? RepresentanteLegal { get; set; }
        public string? DniRepresentante { get; set; }
        public string? LogoUrl { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public ContadorInfoResponse? Contador { get; set; }
    }
    public class SpEmpresaCrearResult
    {
        public Guid? Id { get; set; }
        public bool Success { get; set; }
        public string Message { get; set; }
    }

}
