using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class UpdateOfferSupplyDayDto
    {
        public Guid OfferGuid { get; set; }
        public int NewSupplyDay { get; set; }
    }
}
