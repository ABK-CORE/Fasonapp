
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfDepartmentDal: EfEntityRepositoryBase<Department, ContextDb>, IDepartmentDal
{
    public EfDepartmentDal(ContextDb context) : base(context)
    {
    }
}
