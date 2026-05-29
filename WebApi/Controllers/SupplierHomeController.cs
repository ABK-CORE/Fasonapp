using Business.Abstract;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierHomeController : BaseApiController
    {
        private readonly ISupplierHomeService supplierHomeService;

        public SupplierHomeController(ISupplierHomeService supplierHomeService)
        {
            this.supplierHomeService = supplierHomeService;
        }

        [HttpGet("dashboard-summary")]
        public IActionResult GetSupplierDashboardSummary()
        {
            var result = supplierHomeService.GetSupplierDashboardSummary(CurrentUserGuid);
            return Ok(result);

        }
    }
}
