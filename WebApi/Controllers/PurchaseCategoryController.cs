using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/purchase-category")]
    public class PurchaseCategoryController : BaseApiController
    {
        private readonly IPurchaseCategoryService _service;

        public PurchaseCategoryController(IPurchaseCategoryService service)
        {
            _service = service;
        }

        [HttpGet]
        public IActionResult GetAll()
            => Ok(_service.GetAll());

        [HttpGet("{id}")]
        public IActionResult Get(int id)
            => Ok(_service.GetById(id));

        [RequireDbRole("CategoryManagement")]
        [HttpPost]
        public IActionResult Create(PurchaseCategoryCreateDto dto)
            => Ok(_service.Create(dto));

        [RequireDbRole("CategoryManagement")]
        [HttpPut]
        public IActionResult Update(PurchaseCategoryUpdateDto dto)
            => Ok(_service.Update(dto));

        [RequireDbRole("CategoryManagement")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
            => Ok(_service.Delete(id));
    }
}