using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Business.Concrete
{
    public class PermissionManager : IPermissionService
    {
        private readonly IUserRoleDal _userRoleDal;

        public PermissionManager(
            IUserRoleDal userRoleDal)
        {
            _userRoleDal = userRoleDal;
        }

        public IDataResult<List<string>> GetPermissionsByUser(Guid userGuid)
        {
            // 1. Kullanıcının rol ID'lerini al
            var roleList = _userRoleDal
                .GetList(ur => ur.UserGuid == userGuid)
                .Distinct()
                .ToList();

            var roleText = roleList.Select(ur => ur.Role.RoleName).ToList();

            return new SuccessDataResult<List<string>>(roleText);
        }
    }
}