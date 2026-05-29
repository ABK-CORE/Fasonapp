
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfRequestApprovalRecordDal: EfEntityRepositoryBase<RequestApprovalRecord, ContextDb>, IRequestApprovalRecordDal
{
    public EfRequestApprovalRecordDal(ContextDb context) : base(context)
    {
    }
}
