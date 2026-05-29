
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPurchaseRequestDal: EfEntityRepositoryBase<PurchaseRequest, ContextDb>, IPurchaseRequestDal
{
    public EfPurchaseRequestDal(ContextDb context) : base(context)
    {
    }
}
