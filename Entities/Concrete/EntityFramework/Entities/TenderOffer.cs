using Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPTenderOffer")]
    public class TenderOffer:IEntity
    {
        public int OfferId { get; set; }
        public Guid OfferGuid { get; set; }
        public Guid TenderGuid { get; set; }
        public Guid SupplierId { get; set; }

        public int PartId { get; set; }
        public Part Part { get; set; } = null!;
        [Column(TypeName = "decimal(18,4)")]
        public DateTime CreatedDate { get; set; }
        public bool? IsAccepted { get; set; }
        public int? SupplyDay { get; set; }
        public decimal UnitPrice { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRateToTL { get; set; }
        public decimal PriceInTL { get; set; }
    }
}
