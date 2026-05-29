using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPApprovalRule")]
    public class ApprovalRule : IEntity
    {
        public int RuleId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }

        // Bu kurala atanmış kullanıcılar
        public virtual ICollection<ApprovalRuleApprover> Approvers { get; set; }
            = new List<ApprovalRuleApprover>();
    }
}
