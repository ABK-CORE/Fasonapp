using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderItemCreateDto:IDto
    {
        public int PartId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string? Note { get; set; }
    }
}
