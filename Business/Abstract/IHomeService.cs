using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IHomeService
    {
        IDataResult<PageResult<TenderListDto>> GetPendingApprovals(int page, int pageSize, Guid currentUserGuid);
        IDataResult<PageResult<TenderListDto>> GetPendingOffers(int page, int pageSize);
        IDataResult<List<TenderListDto>> GetTodaysTenders();
        IDataResult<DashboardSummaryDto> GetDashboardSummary(Guid currentUserGuid);
    }
}
