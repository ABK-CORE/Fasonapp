using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class Contract:IEntity
    {
        public int ContractId { get; set; }
        public Guid ContractGuid { get; set; }
        public Guid SupplierGuid { get; set; }
        public Guid CreatedBy { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public bool IsActive { get; set; } = true;
        public User? Supplier { get; set; }
        public User? CreatedByUser { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public Guid? ModifiedBy { get; set; }

        public ContractType ContractType { get; set; } = ContractType.OneTime;
        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
        public bool ReminderSent { get; set; } = false;

        public ICollection<ContractFile> Files { get; set; } = new List<ContractFile>();
        public ICollection<ContractPart> Parts { get; set; } = new List<ContractPart>();
    }
}
