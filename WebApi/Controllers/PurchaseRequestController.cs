using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseRequestController : BaseApiController
    {
        private readonly IPurchaseRequestService _requestService;

        public PurchaseRequestController(IPurchaseRequestService requestService)
        {
            _requestService = requestService;
        }

        /// <summary> Tüm talepleri getirir </summary>
        [HttpGet]
        public IActionResult GetAll()
        {
            var result = _requestService.GetAll(CurrentUserGuid);
            return Ok(result);
        }

        /// <summary> Guid’e göre talebi getirir </summary>
        [HttpGet("{requestGuid:guid}")]
        public IActionResult GetByGuid(Guid requestGuid)
        {
            var result = _requestService.GetByGuid(requestGuid, CurrentUserGuid);
            return Ok(result);
        }

        /// <summary> Yeni talep oluşturur </summary>
        [HttpPost]
        public IActionResult Create([FromBody] PurchaseRequestCreateDto dto)
        {
            var result = _requestService.Create(dto, CurrentUserGuid);
            return Ok(result);
        }

        /// <summary> Sıradaki onay adımını gerçekleştirir </summary>
        [HttpPost("{requestGuid:guid}/approve")]
        public IActionResult Approve(
            Guid requestGuid,
            [FromQuery] bool isApproved
        )
        {
            var result = _requestService.ApproveStep(requestGuid, CurrentUserGuid, isApproved);
            return Ok(result);
        }

        /// <summary> Talebi iptal eder </summary>
        [HttpPost("{requestGuid:guid}/cancel")]
        public IActionResult Cancel(Guid requestGuid)
        {
            var result = _requestService.Cancel(requestGuid, CurrentUserGuid);
            return Ok(result);
        }

        [HttpPost("{requestGuid}/transfer")]
        [RequireDbRole("TenderManager")]
        public IActionResult TransferToTender(
            Guid requestGuid,
            [FromBody] TransferToTenderDto dto
        )
        {
            var result = _requestService.TransferToTender(
                requestGuid,
                CurrentUserGuid,
                dto.IsOfferBased,
                dto.SupplierOffers
            );

            return Ok(result.Message);
        }

        /// <summary> Talebi iptal eder </summary>
        [HttpPost("{requestGuid:guid}/rejectionbybuyer")]
        public IActionResult RejectionbyBuyer(Guid requestGuid)
        {
            var result = _requestService.RejectionbyBuyer(requestGuid, CurrentUserGuid);
            return Ok(result);
        }
    }
}