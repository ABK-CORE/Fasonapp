using Business.Abstract;
using Core.Utilities.Result.Concrete;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TenderController : BaseApiController
    {
        private readonly ITenderService _tenderService;

        public TenderController(ITenderService tenderService)
        {
            _tenderService = tenderService;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _tenderService.GetAllTenders();
            return Ok(result);
        }

        [HttpGet("{tenderGuid:guid}")]
        public IActionResult GetByGuid(Guid tenderGuid)
        {
            // 1) Servisten DTO’yu al
            var result = _tenderService.GetTenderByGuid(tenderGuid);
            if (!result.Success)
                return Ok(result);

            var dto = result.Data!;

            // 2) Eğer kullanıcı Supplier rolündeyse, sadece kendi teklifler
            if (User.IsInRole("Supplier"))
            {
                dto.Offers = dto.Offers!
                    .Where(o => o.SupplierId == CurrentUserGuid)
                    .ToList();
            }

            // 3) Güncellenmiş DTO ile dön
            return Ok(new SuccessDataResult<TenderDto>(dto, result.Message));
        }

        [HttpPost]
        [RequireDbRole("TenderManagement")]
        public IActionResult Create([FromBody] TenderCreateDto dto)
        {
            dto.CreatedBy = CurrentUserGuid;
            var result = _tenderService.CreateTender(dto);
            return Ok(result);
        }

        [HttpDelete("{tenderGuid:guid}")]
        [RequireDbRole("TenderManagement")]
        public IActionResult Delete(Guid tenderGuid)
        {
            var result = _tenderService.DeleteTender(tenderGuid, CurrentUserGuid);
            return Ok(result);
        }

        [HttpGet("paged")]
        public IActionResult GetPaged(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? title = null,
            [FromQuery] Guid? createdBy = null,
            [FromQuery] int? status = null,
            [FromQuery] int? partId = null,
            [FromQuery] Guid? pendingApprover = null
        )
        {
            var filter = new TenderFilterDto
            {
                Title = title,
                CreatedBy = createdBy,
                Status = status,
                PartId = partId,
                PendingApprover = pendingApprover
            };

            var result = _tenderService.GetTendersPaged(page, pageSize, filter);
            return Ok(result);
        }

        [HttpGet("pending-offers")]
        public IActionResult GetPendingOffers()
        {
            var result = _tenderService.GetTendersByStatus(2);

            return Ok(result);
        }

        [HttpPost("{tenderGuid}/offers")]
        public IActionResult SubmitOffers([FromBody] TenderOfferCreateDto dto)
        {
            var result = _tenderService.SubmitOffers(dto, CurrentUserGuid);
            return Ok(result);
        }

        [HttpPost("{tenderGuid:guid}/close/{supplierGuid:guid}")]
        public IActionResult CloseTenderAndStartApproval(Guid tenderGuid, Guid supplierGuid)
        {
            var result = _tenderService.CloseTenderAndStartApproval(
                tenderGuid,
                CurrentUserGuid,
                supplierGuid
            );

            return Ok(result);
        }

        [HttpGet("for-supplier")]
        public IActionResult GetForSupplier()
        {
            var result = _tenderService.GetTendersForSupplier(CurrentUserGuid);

            return Ok(result);
        }

        [HttpGet("getstoklarbystokkodu")]
        public IActionResult GetStoklarByStokKodu(string partCode)
        {
            var result = _tenderService.GetStoklarByStokKodu(partCode);
            return Ok(result);
        }

        [HttpPost("update-tender-count")]
        public IActionResult UpdateTenderCount([FromBody] UpdateTenderCountDto dto)
        {
            var result = _tenderService.UpdateTenderCount(dto);
            return Ok(result);
        }

        [HttpPost("completed")]
        [RequireDbRole("TenderManagement")]
        public IActionResult CompletedTender([FromBody] TenderCompletedDto dto)
        {
            var result = _tenderService.CompletedTender(dto, CurrentUserGuid);
            return Ok(result);
        }

        [HttpPut("offer/update-price")]
        [RequireDbRole("TenderManagement")]
        public IActionResult UpdateOfferPrice([FromBody] UpdateOfferPriceDto dto)
        {
            var result = _tenderService.UpdateOfferPrice(dto, CurrentUserGuid);
            return Ok(result);
        }

        [HttpPut("offer/update-supply-day")]
        [RequireDbRole("TenderManagement")]
        public IActionResult UpdateOfferSupplyDay([FromBody] UpdateOfferSupplyDayDto dto)
        {
            var result = _tenderService.UpdateOfferSupplyDay(dto, CurrentUserGuid);
            return Ok(result);
        }

    }
}
