
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfDepartmentApprovalProcessDal: EfEntityRepositoryBase<DepartmentApprovalProcess, ContextDb>, IDepartmentApprovalProcessDal
{
    public EfDepartmentApprovalProcessDal(ContextDb context) : base(context)
    {
    }
}
