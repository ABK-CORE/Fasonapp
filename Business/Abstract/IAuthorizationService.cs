using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IAuthorizationService
    {
        IDataResult<List<UserWithRolesDto>> GetUsers();
        IDataResult<List<RoleDto>> GetRoles();
        IResult UpdateUserRoles(Guid userGuid, UpdateUserRolesDto dto);
    }
}
