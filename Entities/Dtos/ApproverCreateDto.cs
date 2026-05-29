using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class ApproverCreateDto : IDto
    {
        public Guid UserGuid { get; set; }
        public int OrderIndex { get; set; }
    }

}
