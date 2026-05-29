using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalRuleUpdateDto : IDto
    {
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
        public List<int> ApproverIds { get; set; }
    }
}
