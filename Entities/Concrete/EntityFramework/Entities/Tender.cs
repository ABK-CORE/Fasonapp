using Core.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPTender")]
    public class Tender : IEntity
    {
        public int TenderId { get; set; }
        public Guid TenderGuid { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public int IsOfferBased { get; set; }
        public int CategoryId { get; set; }
        public bool IsPurchaseRequest { get; set; }
        public int? PurchaseId { get; set; } = null;
        public bool IsActive { get; set; } = true;
        public string? DeliveryNoteNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateOnly? DeliveryNoteDate { get; set; }
        public DateOnly? InvoiceDate { get; set; }
        public PurchaseCategory? Category { get; set; } = new PurchaseCategory();
        public ICollection<TenderItem> Items { get; set; } = new List<TenderItem>();
        public ICollection<ApprovalRecord> ApprovalRecords { get; set; } = new List<ApprovalRecord>();
        public virtual ICollection<TenderOffer> Offers { get; set; } = new List<TenderOffer>();
    }
}
