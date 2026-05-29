using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/departments/{departmentGuid:guid}/approval-process")]
    [ApiController]
    public class DepartmentApprovalProcessController : BaseApiController
    {
        private readonly IDepartmentApprovalProcessService _processService;

        public DepartmentApprovalProcessController(
            IDepartmentApprovalProcessService processService)
        {
            _processService = processService;
        }

        /// <summary>
        /// Bir departmanın tanımlı onay adımlarını getirir.
        /// </summary>
        [HttpGet]
        public IActionResult GetSteps(Guid departmentGuid)
        {
            var result = _processService.GetSteps(departmentGuid);

            return Ok(result);
        }

        /// <summary>
        /// Bir departmanın onay adımlarını kaydeder veya günceller.
        /// </summary>
        [RequireDbRole("DepartmentManagement")]
        [HttpPost]
        public IActionResult SaveSteps(
            Guid departmentGuid,
            [FromBody] List<ApprovalProcessStepDto> steps)
        {
            if (steps == null || steps.Count == 0)
                return BadRequest("En az bir onay adımı tanımlanmalıdır.");

            var result = _processService.SaveSteps(departmentGuid, steps);

            return Ok(result);
        }
    }
}
