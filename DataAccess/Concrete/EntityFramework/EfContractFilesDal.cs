
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Concrete.EntityFramework;
public class EfContractFilesDal: EfEntityRepositoryBase<ContractFile, ContextDb>, IContractFilesDal
{
    public EfContractFilesDal(ContextDb context) : base(context)
    {
    }
}
