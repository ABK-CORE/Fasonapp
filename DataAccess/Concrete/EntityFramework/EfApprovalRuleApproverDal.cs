
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfApprovalRuleApproverDal: EfEntityRepositoryBase<ApprovalRuleApprover, ContextDb>, IApprovalRuleApproverDal
{
    public EfApprovalRuleApproverDal(ContextDb context) : base(context)
    {
    }
}
