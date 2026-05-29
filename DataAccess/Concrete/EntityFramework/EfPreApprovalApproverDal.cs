
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPreApprovalApproverDal: EfEntityRepositoryBase<PreApprovalApprover, ContextDb>, IPreApprovalApproverDal
{
    public EfPreApprovalApproverDal(ContextDb context) : base(context)
    {
    }
}
