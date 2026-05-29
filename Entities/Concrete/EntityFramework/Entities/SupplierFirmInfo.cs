using Core.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPSupplierFirmInfo")]
    public class SupplierFirmInfo : IEntity
    {
        [Key]
        public int SupplierFirmInfoId { get; set; }

        [Required]
        public Guid UserGuid { get; set; }

        [ForeignKey(nameof(UserGuid))]
        public virtual User User { get; set; } = null!;

        [ StringLength(200)]
        public string? CompanyName { get; set; }

        [StringLength(100)]
        public string? TaxOffice { get; set; }

        [StringLength(50)]
        public string? TaxNumber { get; set; }

        [StringLength(50)]
        public string? TradeRegisterNumber { get; set; }

        [ StringLength(50)]
        public string? CompanyType { get; set; }

        [StringLength(50)]
        public string? MerisNo { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
