using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderDto:IDto
    {
        public int TenderId { get; set; }
        public Guid TenderGuid { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public Guid CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
        public string CreatedByName { get; set; } = string.Empty;
        public int IsOfferBased { get; set; }
        public List<TenderItemDto> Items { get; set; } = new();
        public List<ApprovalRecordDto> ApprovalRecords { get; set; } = new();
        public List<TenderOfferDto> Offers { get; set; } = new();
        public TenderCategoryDto Category { get; set; }
        public Guid? SelectedSupplierId { get; set; }
        public string? SelectedSupplierName { get; set; }
        public string? SelectedSupplierCompany { get; set; }
        public decimal PeriodUsedQuantity { get; set; }
        public bool IsPeriodLimitExceeded { get; set; }
        public string PeriodLimitMessage { get; set; } = string.Empty;
        public bool IsParticipated { get; set; }
        public bool IsWon { get; set; }
        public bool IsPurchaseRequest { get; set; }
        public Guid? PurchaseGuid { get; set; } = null;
        public string? DeliveryNoteNumber { get; set; }
        public string? InvoiceNumber { get; set; }
        public DateOnly? DeliveryNoteDate { get; set; }
        public DateOnly? InvoiceDate { get; set; }
    }
}
