using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class RequestApprovalRecordDto
    {
        public int OrderIndex { get; set; }
        public StepType StepType { get; set; }
        public Guid? UserGuid { get; set; }
        public int? ManagerLevel { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ActionDate { get; set; }
        public UserDto? Approver { get; set; }
    }
}
