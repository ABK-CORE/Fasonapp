using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class UpdateOfferPriceDto
    {
        public Guid OfferGuid { get; set; }
        public decimal NewUnitPrice { get; set; }
    }
}
