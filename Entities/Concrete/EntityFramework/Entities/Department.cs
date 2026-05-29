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
    [Table("Department")]
    public class Department : IEntity
    {
        [Key]
        public int DepartmentId { get; set; }
        public Guid Guid { get; set; }

        [Required, StringLength(250)]
        public string Name { get; set; } = null!;

        [StringLength(1000)]
        public string? Description { get; set; }

        public DateTime CreatedDate { get; set; }
        public bool IsActive { get; set; }

        public virtual ICollection<DepartmentUser> DepartmentUsers { get; set; } = new List<DepartmentUser>();
        public virtual ICollection<DepartmentApprovalProcess> ApprovalProcesses { get; set; } = new List<DepartmentApprovalProcess>();
    }
}
