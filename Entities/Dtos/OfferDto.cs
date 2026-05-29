using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class OfferDto:IDto
    {
        public int PartId { get; set; }
        public decimal UnitPrice { get; set; }
        public bool UseContractPrice { get; set; }
        public int SupplyDay { get; set; }
        public string Currency { get; set; }
        public decimal ExchangeRate { get; set; }
    }
}
