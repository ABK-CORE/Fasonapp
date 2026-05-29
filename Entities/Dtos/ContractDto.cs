using Core.Entities;
using Entities.Enum;

namespace Entities.Dtos
{
    public class ContractDto:IDto
    {
        public int ContractId { get; set; }
        public Guid ContractGuid { get; set; }
        public Guid SupplierGuid { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<string> FilePath { get; set; } = new();
        public ContractType ContractType { get; set; }
        public RecurrencePattern RecurrencePattern { get; set; }
        public List<PartPriceDto> Parts { get; set; } = new();
    }
}
    