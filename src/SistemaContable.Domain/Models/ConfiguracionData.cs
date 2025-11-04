using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class ConfiguracionData
    {
        public string Tema { get; set; } = "light";
        public string Idioma { get; set; } = "es";
        public bool NotificacionesEmail { get; set; } = true;
        public bool NotificacionesPush { get; set; } = true;
        public string Timezone { get; set; } = "America/Lima";
    }
}
