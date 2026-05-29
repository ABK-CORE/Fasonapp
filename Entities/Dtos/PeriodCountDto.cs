using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class PeriodCountDto : IDto
    {
        public int Created { get; set; }
        public int Completed { get; set; }
    }
}
