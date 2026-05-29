
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfContractsDal: EfEntityRepositoryBase<Contract, ContextDb>, IContractsDal
{
    public EfContractsDal(ContextDb context) : base(context)
    {
    }
}
