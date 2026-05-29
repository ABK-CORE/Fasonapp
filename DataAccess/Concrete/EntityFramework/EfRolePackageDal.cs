
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfRolePackageDal: EfEntityRepositoryBase<RolePackage, ContextDb>, IRolePackageDal
{
    public EfRolePackageDal(ContextDb context) : base(context)
    {
    }
}
