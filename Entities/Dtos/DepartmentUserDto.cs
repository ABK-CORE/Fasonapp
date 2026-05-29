using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class DepartmentUserDto:IDto
    {
        public Guid UserGuid { get; set; }
        public string UserName { get; set; } = null!;
        public bool IsManager { get; set; }
        public int? ManagerLevel { get; set; }
        public DateTime AssignedDate { get; set; }
    }
}
