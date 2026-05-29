using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class PackageRole : IEntity
    {
        public int PackageRoleId { get; set; }
        public int PackageId { get; set; }
        public int RoleId { get; set; }

        public virtual Role Role { get; set; } = null!;
        public virtual RolePackage Package { get; set; } = null!;
    }
}
