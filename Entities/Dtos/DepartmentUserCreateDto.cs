using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DepartmentUserCreateDto:IDto
    {
        public Guid DepartmentGuid { get; set; }
        public Guid UserGuid { get; set; }
        public bool IsManager { get; set; }
        public int? ManagerLevel { get; set; }
    }
}
