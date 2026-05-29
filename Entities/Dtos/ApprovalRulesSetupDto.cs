using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalRulesSetupDto : IDto
    {
        // Tüm fiyat aralıkları için tek seferlik ön-onaycılar
        public List<ApproverDto> PreApprovers { get; set; } = new();

        // Fiyat-temelli onay kuralları
        public List<ApprovalRuleCreateDto> Rules { get; set; } = new();
    }
}
