using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class RolePackage : IEntity
    {
        public int PackageId { get; set; }
        public string PackageName { get; set; } = null!;
        public string? Description { get; set; }
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<PackageRole> PackageRoles { get; set; } = new List<PackageRole>();
        public virtual ICollection<UserPackage> UserPackages { get; set; } = new List<UserPackage>();
    }
}
