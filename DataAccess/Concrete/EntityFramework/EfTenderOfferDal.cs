
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfTenderOfferDal: EfEntityRepositoryBase<TenderOffer, ContextDb>, ITenderOfferDal
{
    public EfTenderOfferDal(ContextDb context) : base(context)
    {
    }
}
