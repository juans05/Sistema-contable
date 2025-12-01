using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Models
{
    public class SpResultado
    {
        public int? OIdFactura { get; set; }
        public int? OIdRegVenta { get; set; }
        public int? OIdDetalle { get; set; }
        public bool OExisteDuplicado { get; set; }
        public bool OAnulado { get; set; }
        public bool OActualizado { get; set; }
        public string OMensaje { get; set; }
    }
}
