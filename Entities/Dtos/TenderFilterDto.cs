using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderFilterDto:IDto
    {
        public string? Title { get; set; }
        public Guid? CreatedBy { get; set; }
        public int? Status { get; set; }
        public int? PartId { get; set; }
        public Guid? PendingApprover { get; set; }
    }
}
