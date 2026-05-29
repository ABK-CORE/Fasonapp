using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/departments")]
    [ApiController]
    public class DepartmentsController : BaseApiController
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _departmentService.GetAllDepartments();

            return Ok(result);
        }

        [HttpGet("{departmentGuid:guid}")]
        public IActionResult GetByGuid(Guid departmentGuid)
        {
            var result = _departmentService.GetDepartmentByGuid(departmentGuid);

            return Ok(result);
        }

        [RequireDbRole("DepartmentManagement")]
        [HttpPost]
        public IActionResult Create([FromBody] DepartmentCreateDto dto)
        {
            var result = _departmentService.CreateDepartment(dto);

            return Ok(result);
        }

        [RequireDbRole("DepartmentManagement")]
        [HttpDelete("{departmentGuid:guid}")]
        public IActionResult Delete(Guid departmentGuid)
        {
            var result = _departmentService.DeleteDepartment(departmentGuid);

            return Ok(result);
        }

        [RequireDbRole("DepartmentManagement")]
        [HttpPost("{departmentGuid:guid}/users")]
        public IActionResult AddUserToDepartment(
            Guid departmentGuid,
            [FromBody] DepartmentUserCreateDto dto)
        {
            // DTO içindeki DepartmentGuid alanını path parametresiyle eşleştiriyoruz
            dto.DepartmentGuid = departmentGuid;
            var result = _departmentService.AddUserToDepartment(dto);

            return Ok(result);
        }

        [RequireDbRole("DepartmentManagement")]
        [HttpDelete("{departmentGuid:guid}/users/{userGuid:guid}")]
        public IActionResult RemoveUserFromDepartment(Guid departmentGuid, Guid userGuid)
        {
            var result = _departmentService.RemoveUserFromDepartment(departmentGuid, userGuid);

            return Ok(result);
        }

        [HttpGet("{departmentGuid:guid}/users")]
        public IActionResult GetDepartmentUsers(Guid departmentGuid)
        {
            var result = _departmentService.GetDepartmentUsers(departmentGuid);

            return Ok(result);
        }
    }
}
