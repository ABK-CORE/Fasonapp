using Core.Entities;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class RequestApprovalRecord : IEntity
    {
        [Key]
        public int RecordId { get; set; }
        public Guid RequestGuid { get; set; }
        public int OrderIndex { get; set; }
        public StepType StepType { get; set; }
        public Guid? UserGuid { get; set; }
        public int? ManagerLevel { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ActionDate { get; set; }

        // navigation
        public virtual PurchaseRequest PurchaseRequest { get; set; } = null!;

        // Yeni eklenen User navigation
        [ForeignKey(nameof(UserGuid))]
        public virtual User? User { get; set; }
    }
}
