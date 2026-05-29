using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using Core.Utilities.Security.JWT;
using Entities.Dtos;

namespace Business.Abstract;

public interface IAccountService
{
    IDataResult<AccessToken> Login(LoginDto loginDto);
    IDataResult<Guid> Register(RegisterDto registerDto);
    IDataResult<List<string>> GetUserRoles(Guid user);
    IResult SendEmailVerificationCode(SendEmailVerificationDto dto);
    IDataResult<AccessToken> ConfirmEmail(ConfirmEmailDto dto);
    IDataResult<bool> SendPasswordResetCode(SendPasswordResetDto dto);
    IDataResult<bool> ResetPassword(ResetPasswordDto dto);
}
