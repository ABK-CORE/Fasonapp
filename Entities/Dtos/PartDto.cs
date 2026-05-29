using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PartDto : IDto
    {
        public int PartId { get; set; }
        public string PartCode { get; set; }
        public string PartName { get; set; }
        public string? Description { get; set; }
        public string? PartPhoto { get; set; }
    }
}
