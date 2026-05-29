using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderOfferDto:IDto
    {
        public int OfferId { get; set; }
        public Guid OfferGuid { get; set; }
        public Guid TenderGuid { get; set; }
        public Guid SupplierId { get; set; }
        public int PartId { get; set; }

        public decimal Price { get; set; }
        public decimal UnitPrice { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool? IsAccepted { get; set; }

        public string? PartName { get; set; }
        public string? PartCode { get; set; }

        public string SupplierName { get; set; } = null!;
        public string? SupplierCompany { get; set; }
        public int? SupplyDay { get; set; }
        public bool IsInContract { get; set; }  
        public string? ContractTitle { get; set; }
        public decimal? ContractPrice { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRateToTL { get; set; }
        public decimal PriceInTL { get; set; }
    }
}
