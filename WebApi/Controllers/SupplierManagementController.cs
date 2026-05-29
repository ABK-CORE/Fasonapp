using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/supplier-management")]
    [ApiController]
    public class SupplierManagementController : ControllerBase
    {
        private readonly ISupplierManagementService _svc;
        public SupplierManagementController(ISupplierManagementService svc)
            => _svc = svc;

        [HttpGet]
        public IActionResult GetAll()
            => Ok(_svc.GetSuppliers());

        [HttpGet("{guid}")]
        public IActionResult Get(Guid guid)
            => Ok(_svc.GetSupplier(guid));

        [HttpPost]
        [RequireDbRole("SupplierManagement")]
        public IActionResult Create(SupplierCreateDto dto)
            => Ok(_svc.CreateSupplier(dto));

        [HttpPut]
        [RequireDbRole("SupplierManagement")]
        public IActionResult Update(SupplierUpdateDto dto)
            => Ok(_svc.UpdateSupplier(dto));

        [HttpDelete("{guid}")]
        [RequireDbRole("SupplierManagement")]
        public IActionResult Delete(Guid guid)
            => Ok(_svc.DeleteSupplier(guid));
    }
}
