
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPackageRoleDal: EfEntityRepositoryBase<PackageRole, ContextDb>, IPackageRoleDal
{
    public EfPackageRoleDal(ContextDb context) : base(context)
    {
    }
}
