
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPurchaseCategoryDal: EfEntityRepositoryBase<PurchaseCategory, ContextDb>, IPurchaseCategoryDal
{
    public EfPurchaseCategoryDal(ContextDb context) : base(context)
    {
    }
}
