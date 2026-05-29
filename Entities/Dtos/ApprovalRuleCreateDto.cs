using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalRuleCreateDto : IDto
    {
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public List<ApproverDto> Approvers { get; set; } = new();
    }

    public class ApproverAssignmentDto
    {
        public Guid UserGuid { get; set; }
        public int OrderIndex { get; set; }
    }
}
