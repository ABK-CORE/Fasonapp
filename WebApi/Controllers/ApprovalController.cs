using Business.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/approval")]
    [ApiController]
    public class ApprovalController : BaseApiController
    {
        private readonly IApprovalService _approvalService;
        public ApprovalController(IApprovalService approvalService)
            => _approvalService = approvalService;

        // POST api/approval/act
        [HttpPost("act")]
        public IActionResult Act([FromBody] ApprovalActionDto dto)
        {
            dto.UserGuid = CurrentUserGuid;
            var result = _approvalService.ActOnApproval(dto);
            return Ok(result);
        }
    }
}
