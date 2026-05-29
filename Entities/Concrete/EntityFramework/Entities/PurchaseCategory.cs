using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPPurchaseCategory")]
    public class PurchaseCategory : IEntity
    {
        [Key]
        public int CategoryId { get; set; }
        public string Name { get; set; } = null!;
        public decimal LimitAmount { get; set; }
        public LimitPeriodType LimitPeriod { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
