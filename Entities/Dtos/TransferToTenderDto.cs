using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TransferToTenderDto
    {
        public int IsOfferBased { get; set; }
        [JsonPropertyName("supplierOffers")]
        public List<TenderCreateOfferDto>? SupplierOffers { get; set; }
    }
}
