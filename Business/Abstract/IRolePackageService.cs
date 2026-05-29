using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IRolePackageService
    {
        IDataResult<List<RolePackageWithRolesDto>> GetAllPackages();
        IResult CreatePackage(CreateRolePackageDto dto);
        IResult UpdatePackageRoles(int packageId, UpdateRolePackageDto dto);
        IResult AssignPackageToUser(AssignRolePackageToUserDto dto);
        IResult DeletePackage(int packageId);
    }
}
