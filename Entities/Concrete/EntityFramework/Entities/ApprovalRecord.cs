using Core.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    [Table("OPApprovalRecord")]
    public class ApprovalRecord:IEntity
    {
        public int ApprovalId { get; set; }
        public Guid TenderGuid { get; set; }
        public Guid UserGuid { get; set; }
        public int OrderIndex { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ActionDate { get; set; }
        public int RecordType { get; set; }
        public Tender Tender { get; set; }
    }
}
