using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PurchaseRequestItemDto
    {
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public string PartCode { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public string? PartDescription { get; set; }
        public string Unit { get; set; }
    }
}
