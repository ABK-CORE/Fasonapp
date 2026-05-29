using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class DepartmentApprovalProcess : IEntity
    {
        public int ProcessId { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Department Department { get; set; } = null!;
        public virtual ICollection<ApprovalProcessStep> Steps { get; set; } = new List<ApprovalProcessStep>();
    }
}
