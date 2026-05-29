using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalRuleDto : IDto
    {
        public int RuleId { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        // Fiyat-temelli onaycılar
        public List<ApproverDto> Approvers { get; set; } = new();

        // Talep aşamasındaki ön-onaycılar
        public List<ApproverDto> PreApprovers { get; set; } = new();
    }
}
