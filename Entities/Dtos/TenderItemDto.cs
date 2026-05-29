using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderItemDto:IDto
    {
        public int Id { get; set; }
        public int PartId { get; set; }
        public decimal Quantity { get; set; }
        public string PartName { get; set; }
        public string Unit { get; set; }
        public string? Note { get; set; }
        public string? Photo { get; set; }
        public string? Code { get; set; }
        public decimal? DeliveredProduct { get; set; }
    }
}
