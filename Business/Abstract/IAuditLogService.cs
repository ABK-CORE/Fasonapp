using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAuditLogService
    {
        IResult Log(
            Guid userGuid,
            string message,
            string type = "General",
            string? category = null,
            int? requestId = null
        );
        IDataResult<List<AuditLogDto>> GetAll();
        IDataResult<List<AuditLogDto>> GetByFilter(int? requestId, string? category, int? take = null);
    }
}
