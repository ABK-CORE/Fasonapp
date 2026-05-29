using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ContractWithPartsDto:IDto
    {
        public int ContractId { get; set; }
        public Guid ContractGuid { get; set; }
        public string Title { get; set; } = string.Empty;
        public List<PartPriceDto> Parts { get; set; } = new();
    }
}
