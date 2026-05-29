
using Core.DataAccess;
using Core.Utilities.Result.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System.Linq.Expressions;

namespace DataAccess.Abstract;
public interface ITenderDal: IEntityRepository<Tender>
{
    IQueryable<Tender> Query(Expression<Func<Tender, bool>>? filter = null, params Expression<Func<Tender, object>>[] includes);
    IDataResult<List<usp_GetStoklarByStokKoduDto>> GetByStokKodu(string stokKodu);
}
