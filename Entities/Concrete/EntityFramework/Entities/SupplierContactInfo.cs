using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPSupplierContactInfo")]
    public class SupplierContactInfo : IEntity
    {
        [Key]
        public int SupplierContactInfoId { get; set; }

        [Required]
        public Guid UserGuid { get; set; }

        [ForeignKey(nameof(UserGuid))]
        public virtual User User { get; set; } = null!;

        [ StringLength(100)]
        public string? ContactName { get; set; }

        [StringLength(100)]
        public string? ContactPosition { get; set; }

        [ StringLength(50)]
        public string PhoneNumber { get; set; }

        [ StringLength(100)]
        public string? Email { get; set; }

        [StringLength(200)]
        public string? Website { get; set; }

        [ StringLength(250)]
        public string? Address { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
