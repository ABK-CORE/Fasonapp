using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/auditlogs")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpGet("logs")]
        public IActionResult GetByFilter([FromQuery] int? requestId,
            [FromQuery] string? category,
            [FromQuery] int? take)
        {
            IDataResult<List<AuditLogDto>> result;

            // Eğer hiç filtre gönderilmemişse tüm kayıtlar
            if (!requestId.HasValue && string.IsNullOrEmpty(category))
                result = _auditLogService.GetAll();
            else
                result = _auditLogService.GetByFilter(requestId, category, take);

            return Ok(result);
        }
    }
}
