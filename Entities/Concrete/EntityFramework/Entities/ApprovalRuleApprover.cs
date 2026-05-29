using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPApprovalRuleApprover")]
    public class ApprovalRuleApprover : IEntity
    {
        public int RuleApproverId { get; set; }

        // Foreign key → ApprovalRule
        public int RuleId { get; set; }
        public virtual ApprovalRule Rule { get; set; } = null!;

        // Atanan kullanıcı (User tablosundaki Guid sütunu)
        public Guid UserGuid { get; set; }

        // Akış içindeki sıra
        public int OrderIndex { get; set; }
    }
}
