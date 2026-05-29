using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PurchaseRequestDto
    {
        public int RequestId { get; set; }
        public Guid RequestGuid { get; set; }
        public int DepartmentId { get; set; }
        public UserDto CreatedByUser { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime RequiredDate { get; set; }
        public RequestStatus Status { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsNextApprover { get; set; }
        public UserDto NextApprover { get; set; } = null!;
        public List<PurchaseRequestItemDto> Items { get; set; }
        public List<RequestApprovalRecordDto> ApprovalRecords { get; set; }
    }
}
