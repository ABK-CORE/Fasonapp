using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class UpdateTenderCountDto
    {
        public Guid Guid { get; set; }
        public decimal Quantity { get; set; }
        public int PartId { get; set; }
    }
}
