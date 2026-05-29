using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PurchaseCategoryCreateDto : IDto
    {
        public string Name { get; set; } = null!;
        public decimal LimitAmount { get; set; }
        public byte LimitPeriod { get; set; }
    }
}
