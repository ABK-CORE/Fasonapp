
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfSupplierContactInfoDal: EfEntityRepositoryBase<SupplierContactInfo, ContextDb>, ISupplierContactInfoDal
{
    public EfSupplierContactInfoDal(ContextDb context) : base(context)
    {
    }
}
