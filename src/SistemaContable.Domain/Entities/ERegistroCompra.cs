using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Domain.Entities
{
    public class ERegistroCompra
    {
        public int IdRegCompras { get; set; }

        public string IdRucProveedor { get; set; }

        public string Periodo { get; set; }

        public string NombreProv { get; set; }

        public string Moneda { get; set; }

        public decimal TipCambio { get; set; }

        public string TipDocumento { get; set; }

        public string SerieDocumento { get; set; }

        public string NoDocumento { get; set; }

        public DateTime FEmisc { get; set; }

        public DateTime FVcto { get; set; }

        public DateTime FContab { get; set; }

        public string Tax { get; set; }

        public string CodDetraccion { get; set; }

        public decimal SubTotal { get; set; }

        public decimal ImpIgv { get; set; }

        public decimal TotalDoc { get; set; }

        public string Glosa { get; set; }

        public string CentroCosto1 { get; set; }

        public string CentroCosto2 { get; set; }

        public string CtaDestino { get; set; }

        public string TipOperaSunat { get; set; }

        public string TipBaseImpSunat { get; set; }

        public string ClasifBienServSunat { get; set; }

        public string OperDeDetrac { get; set; }

        public bool AplicaDetrac { get; set; }

        public string ConcepDetrac { get; set; }

        public string PorcentDetrac { get; set; }

        public decimal MontoDetrac { get; set; }

        public string EstadoDocumento { get; set; }

        public DateTime CreatedAt { get; set; }

        public string UsuarioCreacion { get; set; }

        public virtual EFacturaCompraElectronica FacturaCompraElectronica { get; set; }

        public virtual ICollection<ERegistroCompraDetalle> Detalles { get; set; }
    }
}
