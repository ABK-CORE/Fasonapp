
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfAuditLogDal: EfEntityRepositoryBase<AuditLog, ContextDb>, IAuditLogDal
{
    public EfAuditLogDal(ContextDb context) : base(context)
    {
    }
}
