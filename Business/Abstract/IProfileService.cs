using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IProfileService
    {
        IDataResult<ProfileDto> GetProfile(Guid userGuid);
        IResult UpdateProfile(ProfileDto profileDto, Guid userGuid);
        IResult ChangePassword(ChangePasswordDto changePasswordDto, Guid userGuid);
    }
}
