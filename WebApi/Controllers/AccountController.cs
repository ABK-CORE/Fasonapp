using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers;

[Route("api/user")]
[ApiController]
public class AccountController : BaseApiController
{
    private readonly IAccountService accountService;

    public AccountController(IAccountService accountService)
    {
        this.accountService = accountService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public IActionResult Login(LoginDto loginDto)
    {
        var result = accountService.Login(loginDto);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public IActionResult Register(RegisterDto registerDto)
    {
        var result = accountService.Register(registerDto);
        return Ok(result);
    }

    [HttpGet("getuserroles")]
    public IActionResult GetUserRoles()
    {
        var result = accountService.GetUserRoles(CurrentUserGuid);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("confirm-email")]
    public IActionResult ConfirmEmail([FromBody] ConfirmEmailDto dto)
    {
        var result = accountService.ConfirmEmail(dto);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("send-password-reset-code")]
    public IActionResult SendPasswordResetCode([FromBody] SendPasswordResetDto dto)
    {
        var result = accountService.SendPasswordResetCode(dto);
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    public IActionResult ResetPassword([FromBody] ResetPasswordDto dto)
    {
        var result = accountService.ResetPassword(dto);
        return Ok(result);
    }
}