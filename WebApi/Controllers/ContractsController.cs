using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using Entities.Enum;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace WebApi.Controllers
{
    [Route("api/contracts")]
    [ApiController]
    public class ContractsController : BaseApiController
    {
        private readonly IContractService _contractSvc;

        public ContractsController(
            IContractService contractSvc)
        {
            _contractSvc = contractSvc;
        }

        [HttpGet()]
        public IActionResult GetAll()
        {
            var result = _contractSvc.GetAll();
            return Ok(result);
        }

        [RequireDbRole("ContractManagement")]
        [HttpPost]
        [RequestSizeLimit(10_000_000)]
        public IActionResult Add(
        [FromForm] Guid supplierGuid,
        [FromForm] string title,
        [FromForm] string? description,
        [FromForm] DateTime startDate,
        [FromForm] DateTime endDate,
        [FromForm(Name = "contractFiles")] List<IFormFile>? contractFiles,
        [FromForm] string? parts)
        {
            // DTO oluştur
            var dto = new ContractCreateDto
            {
                SupplierGuid = supplierGuid,
                Title = title,
                Description = description,
                StartDate = startDate,
                EndDate = endDate,
                ContractType = Enum.Parse<ContractType>(Request.Form["contractType"]),
                RecurrencePattern = Enum.Parse<RecurrencePattern>(Request.Form["recurrencePattern"]),
                Parts = !string.IsNullOrEmpty(parts)
                                      ? JsonConvert.DeserializeObject<List<PartPriceDto>>(parts)!
                                      : new()
            };

            var result = _contractSvc.Add(dto, CurrentUserGuid, contractFiles);
            return Ok(result);
        }

        [RequireDbRole("ContractManagement")]
        [HttpDelete("{contractGuid:guid}")]
        public IActionResult Delete(Guid contractGuid)
        {
            var result = _contractSvc.Delete(contractGuid, CurrentUserGuid);
            return Ok(result);
        }

        [HttpGet("for-supplier")]
        public IActionResult GetForSupplier()
        {
            var result = _contractSvc.GetBySupplier(CurrentUserGuid);

            return Ok(result);
        }

        [HttpGet("active-with-parts/{supplierGuid:guid}")]
        public IActionResult GetActiveContractsWithParts(Guid supplierGuid)
        {
            var result = _contractSvc.GetActiveContractsWithParts(supplierGuid);
            return Ok(result);
        }
    }
}