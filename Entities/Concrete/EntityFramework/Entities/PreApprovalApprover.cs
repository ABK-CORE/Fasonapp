using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPPreApprovalApprover")]
    public class PreApprovalApprover:IEntity
    {
        public int PreApproverId { get; set; }
        public Guid UserGuid { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedDate { get; set; }

        public User User { get; set; }
    }
}
