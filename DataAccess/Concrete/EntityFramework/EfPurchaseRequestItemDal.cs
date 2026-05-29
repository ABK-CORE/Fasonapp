
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPurchaseRequestItemDal: EfEntityRepositoryBase<PurchaseRequestItem, ContextDb>, IPurchaseRequestItemDal
{
    public EfPurchaseRequestItemDal(ContextDb context) : base(context)
    {
    }
}
