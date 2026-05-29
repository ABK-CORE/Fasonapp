
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfPartDal: EfEntityRepositoryBase<Part, ContextDb>, IPartDal
{
    public EfPartDal(ContextDb context) : base(context)
    {
    }
}
