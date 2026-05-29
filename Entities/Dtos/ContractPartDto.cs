using Core.Entities;

namespace Entities.Dtos
{
    public class ContractPartDto:IDto
    {
        public int PartId { get; set; }
        public string PartName { get; set; } = null!;
        public decimal UnitPrice { get; set; }
    }
}
