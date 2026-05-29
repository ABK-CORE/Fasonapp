using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DepartmentCreateDto:IDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
