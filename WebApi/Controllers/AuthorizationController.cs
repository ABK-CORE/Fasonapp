using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/authorization")]
    [ApiController]
    public class AuthorizationController : ControllerBase
    {
        private readonly IAuthorizationService _authService;

        public AuthorizationController(IAuthorizationService authService)
        {
            _authService = authService;
        }

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var result = _authService.GetUsers();
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [HttpGet("roles")]
        public IActionResult GetRoles()
        {
            var result = _authService.GetRoles();
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        [RequireDbRole("Authorization")]
        [HttpPost("users/{guid}/roles")]
        public IActionResult UpdateUserRoles(Guid guid, [FromBody] UpdateUserRolesDto dto)
        {
            var result = _authService.UpdateUserRoles(guid, dto);
            if (!result.Success) return BadRequest(result.Message);
            return NoContent();
        }
    }
}
