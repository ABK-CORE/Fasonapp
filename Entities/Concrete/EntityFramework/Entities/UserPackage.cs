using Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Concrete.EntityFramework.Entities
{
    public class UserPackage : IEntity
    {
        public int UserPackageId { get; set; }
        public Guid UserGuid { get; set; }
        public int PackageId { get; set; }
        public DateTime AssignedDate { get; set; }

        public virtual User User { get; set; } = null!;
        public virtual RolePackage Package { get; set; } = null!;
    }
}
