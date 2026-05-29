using Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPPart")]
    public class Part : IEntity
    {
        public int PartId { get; set; }
        public string PartCode { get; set; } = null!;
        public string PartName { get; set; } = null!;
        public string? Description { get; set; }
        public string? PartPhoto { get; set; }
        public bool IsActive { get; set; }
    }
}
