using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("DepartmentUser")]
    public class DepartmentUser : IEntity
    {
        [Key]
        public int DepartmentUserId { get; set; }

        [ForeignKey("Department")]
        public int DepartmentId { get; set; }

        [ForeignKey("User")]
        public Guid UserGuid { get; set; }

        public bool IsManager { get; set; }
        public int? ManagerLevel { get; set; }
        public DateTime AssignedDate { get; set; }

        [ForeignKey(nameof(DepartmentId))]
        public virtual Department Department { get; set; } = null!;

        [ForeignKey(nameof(UserGuid))]
        public virtual User User { get; set; } = null!;
    }
}
