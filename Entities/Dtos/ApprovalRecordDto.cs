using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApprovalRecordDto
    {
        public Guid UserGuid { get; set; }
        public int OrderIndex { get; set; }
        public bool? IsApproved { get; set; }
        public DateTime? ActionDate { get; set; }
        public string UserName { get; set; }
        public int RecordType { get; set; }
    }
}
