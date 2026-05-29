using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class PurchaseRequest : IEntity
    {
        [Key]
        public int RequestId { get; set; }

        [Required]
        public Guid RequestGuid { get; set; } = Guid.NewGuid();

        [Required]
        public int DepartmentId { get; set; }

        [Required]
        public Guid CreatedBy { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        [Required, MaxLength(250)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public DateTime RequiredDate { get; set; }
        public bool IsTransferredToTender { get; set; } = false;
        public int? TenderId { get; set; } = null;

        [Required]
        public RequestStatus Status { get; set; } = RequestStatus.PendingApproval;
        public int CategoryId { get; set; }
        public PurchaseCategory Category { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;

        public virtual ICollection<PurchaseRequestItem> Items { get; set; }
            = new List<PurchaseRequestItem>();

        public virtual ICollection<RequestApprovalRecord> ApprovalRecords { get; set; }
            = new List<RequestApprovalRecord>();
    }
}
