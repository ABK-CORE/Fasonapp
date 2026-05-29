using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class AssignRolePackageToUserDto
    {
        public Guid UserGuid { get; set; }
        public int PackageId { get; set; }
    }
}
