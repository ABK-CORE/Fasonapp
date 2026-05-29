using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class TenderCategoryDto:IDto
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
