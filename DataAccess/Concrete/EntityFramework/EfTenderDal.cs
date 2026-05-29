
using Core.DataAccess.EntityFramework;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Context;
using Entities.Concrete.EntityFramework.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using Core.Utilities.Result.Concrete;
using Microsoft.Data.SqlClient;

namespace DataAccess.Concrete.EntityFramework;
public class EfTenderDal: EfEntityRepositoryBase<Tender, ContextDb>, ITenderDal
{
    private readonly ContextDb Context;
    public EfTenderDal(ContextDb context) : base(context)
    {
        Context = context;
    }

    public IDataResult<List<usp_GetStoklarByStokKoduDto>> GetByStokKodu(string stokKodu)
    {
        var param = new SqlParameter("@StokKodu", stokKodu);
        var liste = Context.usp_GetStoklarByStokKodu
            .FromSqlRaw("EXEC dbo.usp_GetStoklarByStokKodu @StokKodu", param)
            .ToList();

        return new SuccessDataResult<List<usp_GetStoklarByStokKoduDto>>(liste);
    }

    public IQueryable<Tender> Query(
    Expression<Func<Tender, bool>>? filter = null,
    params Expression<Func<Tender, object>>[] includes)
    {
        IQueryable<Tender> query = Context.Set<Tender>();
        if (filter != null)
            query = query.Where(filter);
        foreach (var include in includes)
            query = query.Include(include);
        return query;
    }
}
