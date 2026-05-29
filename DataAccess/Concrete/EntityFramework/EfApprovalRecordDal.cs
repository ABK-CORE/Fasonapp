
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfApprovalRecordDal: EfEntityRepositoryBase<ApprovalRecord, ContextDb>, IApprovalRecordDal
{
    public EfApprovalRecordDal(ContextDb context) : base(context)
    {
    }
}
