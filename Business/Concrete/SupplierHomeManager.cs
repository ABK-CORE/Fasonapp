using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class SupplierHomeManager : ISupplierHomeService
    {
        private readonly IContractsDal _contractDal;
        private readonly ITenderDal _tenderDal;
        private readonly ITenderOfferDal _offerDal;

        public SupplierHomeManager(
            IContractsDal contractDal,
            ITenderDal tenderDal,
            ITenderOfferDal offerDal)
        {
            _contractDal = contractDal;
            _tenderDal = tenderDal;
            _offerDal = offerDal;
        }

        public IDataResult<SupplierDashboardSummaryDto> GetSupplierDashboardSummary(Guid supplierGuid)
        {
            var today = DateTime.Now.Date;
            var start7 = today.AddDays(-6);

            // 1) Toplam aktif sözleşme sayısı
            var totalContracts = _contractDal
                .GetList(c => c.SupplierGuid == supplierGuid && c.IsActive)
                .Count;

            // 2) Devam eden ihaleler (Status 2 veya 3 ve aktif)
            var activeTenders = _tenderDal
                .GetList(t => t.IsActive && (t.Status == 2 || t.Status == 3))
                .Count;

            // 3) Henüz teklif girilmemiş ihaleler (Status=2 ve bu tedarikçi için teklif yok)
            var pendingOffers = _tenderDal
                .GetList(t => t.Status == 2 && t.IsActive)
                .Where(t => !_offerDal.GetList(o => o.TenderGuid == t.TenderGuid && o.SupplierId == supplierGuid).Any())
                .Count();

            // 4) Kabul edilmiş teklifler (IsAccepted == true)
            var acceptedOffers = _offerDal
                .GetList(o => o.SupplierId == supplierGuid && o.IsAccepted == true)
                .Count;

            // 5) Son 7 günde yeni sözleşmeler
            var contractGroups = _contractDal
                .GetList(c => c.SupplierGuid == supplierGuid && c.CreatedDate.Date >= start7)
                .GroupBy(c => c.CreatedDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var newContractsDays = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = start7.AddDays(i);
                    return new DayCountDto
                    {
                        Day = d.ToString("dddd", new CultureInfo("tr-TR")),
                        Count = contractGroups.ContainsKey(d) ? contractGroups[d] : 0
                    };
                }).ToList();

            // 6) Son 7 günde girilen teklifler
            var offerGroups = _offerDal
                .GetList(o => o.SupplierId == supplierGuid && o.CreatedDate.Date >= start7)
                .GroupBy(o => o.CreatedDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            var newOffersDays = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = start7.AddDays(i);
                    return new DayCountDto
                    {
                        Day = d.ToString("dddd", new CultureInfo("tr-TR")),
                        Count = offerGroups.ContainsKey(d) ? offerGroups[d] : 0
                    };
                }).ToList();

            var summary = new SupplierDashboardSummaryDto
            {
                TotalContracts = totalContracts,
                ActiveTendersCount = activeTenders,
                PendingOffersCount = pendingOffers,
                AcceptedOffersCount = acceptedOffers,
                NewContractsLast7Days = newContractsDays,
                NewOffersLast7Days = newOffersDays
            };

            return new SuccessDataResult<SupplierDashboardSummaryDto>(summary);
        }
    }
}
