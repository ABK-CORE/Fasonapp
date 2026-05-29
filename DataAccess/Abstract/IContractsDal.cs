
using Core.DataAccess;
using Entities.Concrete.EntityFramework.Entities;

namespace DataAccess.Abstract;
public interface IContractsDal: IEntityRepository<Entities.Concrete.EntityFramework.Entities.Contract>
{
}
