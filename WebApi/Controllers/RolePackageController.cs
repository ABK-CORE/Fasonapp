using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/rolepackages")]
    [ApiController]
    public class RolePackageController : ControllerBase
    {
        private readonly IRolePackageService _service;

        public RolePackageController(IRolePackageService service)
        {
            _service = service;
        }

        // GET: api/rolepackages
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _service.GetAllPackages();
            if (!result.Success) return BadRequest(result.Message);
            return Ok(result.Data);
        }

        // POST: api/rolepackages
        [HttpPost]
        public IActionResult Create([FromBody] CreateRolePackageDto dto)
        {
            var result = _service.CreatePackage(dto);
            if (!result.Success) return BadRequest(result.Message);
            return CreatedAtAction(nameof(GetAll), null);
        }

        // PUT: api/rolepackages/{packageId}/roles
        [HttpPut("{packageId}/roles")]
        public IActionResult UpdateRoles(int packageId, [FromBody] UpdateRolePackageDto dto)
        {
            var result = _service.UpdatePackageRoles(packageId, dto);
            if (!result.Success) return BadRequest(result.Message);
            return NoContent();
        }

        // POST: api/rolepackages/assign
        [HttpPost("assign")]
        public IActionResult AssignToUser([FromBody] AssignRolePackageToUserDto dto)
        {
            var result = _service.AssignPackageToUser(dto);
            if (!result.Success) return BadRequest(result.Message);
            return NoContent();
        }

        [HttpDelete("{packageId}")]
        public IActionResult Delete(int packageId)
        {
            var result = _service.DeletePackage(packageId);
            if (!result.Success)
                return BadRequest(result.Message);
            return NoContent();
        }
    }
}
