using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPTenderItem")]
    public class TenderItem : IEntity
    {
        public int TenderItemId { get; set; }
        public Guid TenderGuid { get; set; }
        public int PartId { get; set; }
        public decimal Quantity { get; set; }
        public string Unit { get; set; }
        public string? Note { get; set; }
        public decimal? DeliveredProduct { get; set; }

        public Tender Tender { get; set; }
    }
}
