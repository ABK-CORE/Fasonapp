using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface ITenderService
    {
        IDataResult<List<TenderDto>> GetAllTenders();
        IDataResult<TenderDto> CreateTender(TenderCreateDto dto);
        IResult DeleteTender(Guid tenderGuid, Guid user);
        IDataResult<TenderDto> GetTenderByGuid(Guid tenderGuid);
        IDataResult<PageResult<TenderDto>> GetTendersPaged(int page, int pageSize, TenderFilterDto? filter = null);
        IDataResult<List<TenderListDto>> GetTendersByStatus(int status);
        IResult SubmitOffers(TenderOfferCreateDto dto, Guid currentUserGuid);
        IResult CloseTenderAndStartApproval(Guid tenderGuid, Guid currentUserGuid, Guid selectedSupplierGuid);
        IDataResult<List<TenderDto>> GetTendersForSupplier(Guid supplierGuid);
        IDataResult<List<usp_GetStoklarByStokKoduDto>> GetStoklarByStokKodu(string partCode);
        IDataResult<bool> UpdateTenderCount(UpdateTenderCountDto dto);
        IResult CompletedTender(TenderCompletedDto dto, Guid CurrentUserGuid);
        IResult UpdateOfferPrice(UpdateOfferPriceDto dto, Guid currentUserGuid);
        IResult UpdateOfferSupplyDay(UpdateOfferSupplyDayDto dto, Guid currentUserGuid);
    }
}
