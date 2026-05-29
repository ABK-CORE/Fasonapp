using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PartPriceDto:IDto
    {
        public int PartId { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
