using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class CreateRolePackageDto
    {
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public List<int> RoleIds { get; set; } = new();
    }
}
