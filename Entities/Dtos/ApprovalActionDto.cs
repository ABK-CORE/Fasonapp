using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalActionDto : IDto
    {
        public Guid TenderGuid { get; set; }
        public Guid UserGuid { get; set; }
        public bool IsApproved { get; set; }
        public Guid? selectedSupplierId { get; set; } = Guid.Empty;
    }
}
