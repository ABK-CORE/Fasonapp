
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfSupplierFirmInfoDal: EfEntityRepositoryBase<SupplierFirmInfo, ContextDb>, ISupplierFirmInfoDal
{
    public EfSupplierFirmInfoDal(ContextDb context) : base(context)
    {
    }
}
