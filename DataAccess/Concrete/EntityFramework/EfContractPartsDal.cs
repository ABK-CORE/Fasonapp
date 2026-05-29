
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfContractPartsDal: EfEntityRepositoryBase<ContractPart, ContextDb>, IContractPartsDal
{
    public EfContractPartsDal(ContextDb context) : base(context)
    {
    }
}
