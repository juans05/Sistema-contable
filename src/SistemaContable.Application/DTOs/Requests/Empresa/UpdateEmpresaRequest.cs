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
        public Guid Id { get; set; }

        [Required(ErrorMessage = "La razón social es requerida")]
        [StringLength(255)]
        public string RazonSocial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre comercial es requerido")]
        [StringLength(255)]
        public string NombreComercial { get; set; } = string.Empty;

        [Required(ErrorMessage = "El RUC es requerido")]
        [StringLength(20, MinimumLength = 11)]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "El RUC debe contener 11 dígitos numéricos")]
        public string Ruc { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Direccion { get; set; }

        [StringLength(50)]
        [Phone]
        public string? Telefono { get; set; }

        [EmailAddress]
        [StringLength(255)]
        public string? Email { get; set; }

        [StringLength(255)]
        [Url]
        public string? Web { get; set; }

        [StringLength(50)]
        public string? RegimenTributario { get; set; }

        [StringLength(50)]
        public string? TipoContribuyente { get; set; }

        public DateTime? FechaConstitucion { get; set; }

        [StringLength(255)]
        public string? RepresentanteLegal { get; set; }

        [StringLength(20)]
        [RegularExpression(@"^\d{8}$")]
        public string? DniRepresentante { get; set; }

        [StringLength(500)]
        [Url]
        public string? LogoUrl { get; set; }

      //  public EmpresaConfigDto? Config { get; set; }
    }
}
