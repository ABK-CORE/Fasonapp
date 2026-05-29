using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace WebApi.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
            => _userService = userService;

        // GET api/user/brief-list
        [RequireDbRole("User")]
        [HttpGet("brief-list")]
        public IActionResult GetBriefList()
        {
            return Ok(_userService.GetUserBriefList());
        }
    }
}
