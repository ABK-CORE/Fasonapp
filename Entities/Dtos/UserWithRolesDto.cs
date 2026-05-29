using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Dtos
{
    public class UserWithRolesDto
    {
        public Guid Guid { get; set; }
        public string Email { get; set; } = null!;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public List<RoleDto> Roles { get; set; } = new();

        public int? PackageId { get; set; }
        public string? PackageName { get; set; }
    }
}
