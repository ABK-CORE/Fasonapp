using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPurchaseRequestService
    {
        IDataResult<List<PurchaseRequestDto>> GetAll(Guid currentUserGuid);
        IDataResult<PurchaseRequestDto> GetByGuid(Guid requestGuid, Guid currentUserGuid);
        IResult Create(PurchaseRequestCreateDto dto, Guid createdBy);
        IResult ApproveStep(Guid requestGuid, Guid approverGuid, bool isApproved);
        IResult Cancel(Guid requestGuid, Guid canceledBy);
        IResult TransferToTender(Guid requestGuid, Guid completedBy, int isOfferBased, List<TenderCreateOfferDto>? supplierOffers);
        IResult RejectionbyBuyer(Guid requestGuid, Guid currentByGuid);
    }
}
