using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UserManager : IUserService
    {
        private readonly IUserDal _userDal;
        private readonly IUserRoleDal _userRoleDal;
        private readonly IRoleDal _roleDal;
        private readonly IMapper _mapper;

        public UserManager(
            IUserDal userDal,
            IUserRoleDal userRoleDal,
            IRoleDal roleDal,
            IMapper mapper)
        {
            _userDal = userDal;
            _userRoleDal = userRoleDal;
            _roleDal = roleDal;
            _mapper = mapper;
        }

        public IDataResult<List<UserBriefDto>> GetUserBriefList()
        {
            // 1. "User" rolünü al
            var userRole = _roleDal.Get(r => r.RoleName == "User");
            if (userRole == null)
                return new ErrorDataResult<List<UserBriefDto>>(null, "User rolü bulunamadı.");

            // 2. O role atanmış kullanıcı GUID'lerini çek
            var guids = _userRoleDal
                .GetList(ur => ur.RoleId == userRole.RoleId)
                .Select(ur => ur.UserGuid)
                .Distinct()
                .ToList();

            // 3. Bu GUID'lere sahip kullanıcıları listele
            var users = _userDal
                .GetList(u => guids.Contains(u.Guid))
                .Select(u => new UserBriefDto
                {
                    Id = u.Id,
                    Guid = u.Guid,
                    Username = $"{u.FirstName} {u.LastName}"
                })
                .ToList();

            return new SuccessDataResult<List<UserBriefDto>>(users);
        }
    }
}
