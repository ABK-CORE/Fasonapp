using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ContractCreateDto:IDto
    {
        public Guid SupplierGuid { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? FilePath { get; set; }
        public ContractType ContractType { get; set; } = ContractType.OneTime;
        public RecurrencePattern RecurrencePattern { get; set; } = RecurrencePattern.None;
        public List<PartPriceDto> Parts { get; set; } = new();
    }
}
