using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderListDto:IDto
    {
        public int TenderId { get; set; }
        public Guid TenderGuid { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CreatedByName { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public int Status { get; set; }
    }
}
