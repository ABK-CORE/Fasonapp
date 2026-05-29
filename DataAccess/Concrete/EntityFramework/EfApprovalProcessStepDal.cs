
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfApprovalProcessStepDal: EfEntityRepositoryBase<ApprovalProcessStep, ContextDb>, IApprovalProcessStepDal
{
    public EfApprovalProcessStepDal(ContextDb context) : base(context)
    {
    }
}
