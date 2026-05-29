
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfDepartmentUserDal: EfEntityRepositoryBase<DepartmentUser, ContextDb>, IDepartmentUserDal
{
    public EfDepartmentUserDal(ContextDb context) : base(context)
    {
    }
}
