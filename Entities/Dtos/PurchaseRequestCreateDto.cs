using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PurchaseRequestCreateDto
    {
        public int DepartmentId { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public DateTime RequiredDate { get; set; }
        public int CategoryId { get; set; }
        public List<PurchaseRequestItemCreateDto> Items { get; set; }
            = new List<PurchaseRequestItemCreateDto>();
    }
}
