using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.Empresa
{
    public class CreateEmpresaRequest
    {
        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(255, ErrorMessage = "La razón social no puede exceder 255 caracteres")]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(255, ErrorMessage = "El nombre comercial no puede exceder 255 caracteres")]
        public string NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es requerido")]
        [StringLength(20, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 caracteres")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe contener 11 dígitos numéricos")]
        public string Ruc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        public string? Telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(255)]
        public string? Web { get; set; }

        [StringLength(50)]
        public string? RegimenTributario { get; set; }

        [StringLength(50)]
        public string? TipoContribuyente { get; set; }

        public string? FechaConstitucion { get; set; }

        [StringLength(255)]
        public string? RepresentanteLegal { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos")]
        public string? DniRepresentante { get; set; }

        [StringLength(500)]
        public string? LogoUrl { get; set; }

        //public EmpresaConfigDto? Config { get; set; }

        public int? ContadorId { get; set; }
    }
}
