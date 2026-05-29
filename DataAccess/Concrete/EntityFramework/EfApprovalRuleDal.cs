
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfApprovalRuleDal: EfEntityRepositoryBase<ApprovalRule, ContextDb>, IApprovalRuleDal
{
    public EfApprovalRuleDal(ContextDb context) : base(context)
    {
    }
}
