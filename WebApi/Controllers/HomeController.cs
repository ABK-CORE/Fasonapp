using Business.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : BaseApiController
    {
        private readonly IHomeService _homeService;
        public HomeController(IHomeService homeService)
            => _homeService = homeService;

        [HttpGet("pending-approvals")]
        public IActionResult GetPendingApprovals([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = _homeService.GetPendingApprovals(page, pageSize, CurrentUserGuid);
            return Ok(result);
        }

        [HttpGet("pending-offers")]
        public IActionResult GetPendingOffers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var result = _homeService.GetPendingOffers(page, pageSize);
            return Ok(result);
        }

        [HttpGet("today-tenders")]
        public IActionResult GetTodaysTenders()
        {
            var result = _homeService.GetTodaysTenders();
            return Ok(result);
        }

        [HttpGet("summary")]
        public IActionResult GetDashboardSummary()
        {
            var result = _homeService.GetDashboardSummary(CurrentUserGuid);
            return Ok(result);
        }
    }
}
