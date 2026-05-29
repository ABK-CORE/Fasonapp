using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class RolePackageWithRolesDto
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public List<RoleDto> Roles { get; set; } = new();
    }
}
