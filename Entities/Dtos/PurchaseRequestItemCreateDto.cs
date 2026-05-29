using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{

    public class PurchaseRequestItemCreateDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
    }
}
