using Core.Utilities.Result.Abstract;
using System;
using System.Collections.Generic;

namespace Business.Abstract
{
    public interface IPermissionService
    {
        IDataResult<List<string>> GetPermissionsByUser(Guid userGuid);
    }
}