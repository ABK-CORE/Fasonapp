using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Controllers;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : BaseApiController
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        public IActionResult GetProfile()
        {
            var result = _profileService.GetProfile(CurrentUserGuid);
            return Ok(result);
        }

        [HttpPut]
        public IActionResult UpdateProfile([FromBody] ProfileDto dto)
        {
            var result = _profileService.UpdateProfile(dto, CurrentUserGuid);
            return Ok(result);
        }

        [HttpPut("password")]
        public IActionResult ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var result = _profileService.ChangePassword(dto, CurrentUserGuid);
            return Ok(result);
        }
    }
}
