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
        public string razonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(255, ErrorMessage = "El nombre comercial no puede exceder 255 caracteres")]
        public string nombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es requerido")]
        [StringLength(20, MinimumLength = 11, ErrorMessage = "El RUC debe tener 11 caracteres")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe contener 11 dígitos numéricos")]
        public string ruc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? direccion { get; set; }

        [StringLength(50)]
        [Phone(ErrorMessage = "El teléfono no es válido")]
        public string? telefono { get; set; }

        [EmailAddress(ErrorMessage = "El email no es válido")]
        [StringLength(255)]
        public string? email { get; set; }

        [StringLength(255)]
        public string? web { get; set; }

        [StringLength(50)]
        public string? regimenTributario { get; set; }

        [StringLength(50)]
        public string? tipoContribuyente { get; set; }

        public string? fechaConstitucion { get; set; }

        [StringLength(255)]
        public string? representanteLegal { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener 8 dígitos")]
        public string? dniRepresentante { get; set; }

        [StringLength(500)]
        public string? logoUrl { get; set; }

        //public EmpresaConfigDto? config { get; set; }

        public int? contadorId { get; set; }
    }
}
