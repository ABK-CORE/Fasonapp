
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfTenderItemDal: EfEntityRepositoryBase<TenderItem, ContextDb>, ITenderItemDal
{
    public EfTenderItemDal(ContextDb context) : base(context)
    {
    }
}
