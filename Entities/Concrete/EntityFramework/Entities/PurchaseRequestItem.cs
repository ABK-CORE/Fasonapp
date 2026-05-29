using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class PurchaseRequestItem : IEntity
    {
        [Key]
        public int ItemId { get; set; }
        public Guid RequestGuid { get; set; }
        public int PartId { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; }
        [ForeignKey(nameof(RequestGuid))]
        public PurchaseRequest PurchaseRequest { get; set; } = null!;
        [ForeignKey(nameof(PartId))]
        public Part Part { get; set; } = null!;
    }
}
