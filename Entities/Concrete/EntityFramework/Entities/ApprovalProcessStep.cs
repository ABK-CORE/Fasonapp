using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class ApprovalProcessStep : IEntity
    {
        public int StepId { get; set; }
        public int ProcessId { get; set; }
        public int OrderIndex { get; set; }
        public StepType StepType { get; set; }        // enum StepType { User = 0, ManagerLevel = 1 }
        public Guid? UserGuid { get; set; }
        public int? ManagerLevel { get; set; }

        public virtual DepartmentApprovalProcess Process { get; set; } = null!;
    }
}
