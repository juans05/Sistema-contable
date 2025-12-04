using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Application.DTOs.Requests.Empresa
{
    public class UpdateEmpresaRequest
    {
        [Required(ErrorMessage = "El ID es requerido")]
        public Guid id { get; set; }

        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(255)]
        public string razonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(255)]
        public string nombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es requerido")]
        [StringLength(20, MinimumLength = 11)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe contener 11 dígitos numéricos")]
        public string ruc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? direccion { get; set; }

        [StringLength(50)]
        [Phone]
        public string? telefono { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? email { get; set; }

        [StringLength(255)]
        [Url]
        public string? web { get; set; }

        [StringLength(50)]
        public string? regimenTributario { get; set; }

        [StringLength(50)]
        public string? tipoContribuyente { get; set; }

        public DateTime? fechaConstitucion { get; set; }

        [StringLength(255)]
        public string? representanteLegal { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe contener 8 dígitos numéricos")]
        public string? dniRepresentante { get; set; }

        [StringLength(500)]
        [Url]
        public string? logoUrl { get; set; }

        //  public EmpresaConfigDto? Config { get; set; }
    }
}
