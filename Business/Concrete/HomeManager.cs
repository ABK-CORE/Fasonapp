using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;
using System.Globalization;
using System.Linq;

namespace Business.Concrete
{
    public class HomeManager : IHomeService
    {
        private readonly ITenderDal _tenderDal;
        private readonly IApprovalRecordDal _approvalRecordDal;
        private readonly IUserDal _userDal;
        private readonly IPartDal _partDal;
        private readonly IMapper _mapper;

        public HomeManager(
            ITenderDal tenderDal,
            IApprovalRecordDal approvalRecordDal,
            IUserDal userDal,
            IPartDal partDal,
            IMapper mapper)
        {
            _tenderDal = tenderDal;
            _approvalRecordDal = approvalRecordDal;
            _userDal = userDal;
            _partDal = partDal;
            _mapper = mapper;
        }

        public IDataResult<PageResult<TenderListDto>> GetPendingApprovals(int page, int pageSize, Guid currentUserGuid)
        {
            // 0) Aktif tender GUID’lerini topla
            var aktifTenderGuids = _tenderDal
                .GetList(t => t.IsActive)
                .Select(t => t.TenderGuid)
                .ToHashSet();

            // 1) Henüz onaylanmamış, aktif tender’lara ait approval kayıtları
            var bekleyenKayitlar = _approvalRecordDal
                .GetList(r => r.IsApproved == null && aktifTenderGuids.Contains(r.TenderGuid))
                .ToList();

            // 2) Her tender için en küçük OrderIndex’li (sıradaki) kaydı bul
            var siradakiKayitlar = bekleyenKayitlar
                .GroupBy(r => r.TenderGuid)
                .Select(g => g.OrderBy(r => r.OrderIndex).First())
                .ToList();

            // 3) Size denk gelen (currentUserGuid) kayıtları filtrele
            var banaAitKayitlar = siradakiKayitlar
                .Where(r => r.UserGuid == currentUserGuid)
                .OrderBy(r => r.OrderIndex)
                .ToList();

            // 4) Toplam count
            var total = banaAitKayitlar.Count;

            // 5) Sayfalama uygula
            var sayfaKayitlari = banaAitKayitlar
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // 6) DTO’lara dönüştür, null’ları eleriz
            var dtolar = sayfaKayitlari
                .Select(r =>
                {
                    var tender = _tenderDal.Get(t => t.TenderGuid == r.TenderGuid && t.IsActive);
                    if (tender == null) return null;

                    var creator = _userDal.Get(u => u.Guid == tender.CreatedBy);
                    return new TenderListDto
                    {
                        TenderId = tender.TenderId,
                        TenderGuid = tender.TenderGuid,
                        Title = tender.Title,
                        CreatedDate = tender.CreatedDate,
                        Status = tender.Status,
                        CreatedByName = creator == null
                                           ? "—"
                                           : $"{creator.FirstName} {creator.LastName}"
                    };
                })
                .Where(dto => dto != null)!
                .ToList();

            // 7) Sonuç sayfa objesi
            var result = new PageResult<TenderListDto>
            {
                Items = dtolar,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return new SuccessDataResult<PageResult<TenderListDto>>(result);
        }

        public IDataResult<PageResult<TenderListDto>> GetPendingOffers(int page, int pageSize)
        {
            // 1) Tüm tender’ları çek
            var all = _tenderDal.GetList(x=> x.Status == 2 && x.IsActive).ToList();

            // 2) Teklifi henüz hiç gelmemiş olanları filtrele
            var withoutOffers = all;

            var total = withoutOffers.Count;
            var pageTenders = withoutOffers
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var dtos = pageTenders.Select(t =>
            {
                var creator = _userDal.Get(u => u.Guid == t.CreatedBy);
                return new TenderListDto
                {
                    TenderId = t.TenderId,
                    TenderGuid = t.TenderGuid,
                    Title = t.Title,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    CreatedByName = creator != null
                        ? $"{creator.FirstName} {creator.LastName}"
                        : "—"
                };
            }).ToList();

            var result = new PageResult<TenderListDto>
            {
                Items = dtos,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };

            return new SuccessDataResult<PageResult<TenderListDto>>(result);
        }

        public IDataResult<List<TenderListDto>> GetTodaysTenders()
        {
            // Yerel saate göre "bugün"ün başlangıcı
            var today = DateTime.Now.Date;

            // 1) Bugün oluşturulmuş tüm tender'ları getir
            var todays = _tenderDal
                .GetList(t => t.CreatedDate.Date == today && t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
                .ToList();

            // 2) DTO'ya dönüştür
            var dtos = todays.Select(t =>
            {
                var creator = _userDal.Get(u => u.Guid == t.CreatedBy);
                return new TenderListDto
                {
                    TenderGuid = t.TenderGuid,
                    Title = t.Title,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    CreatedByName = creator != null
                                      ? $"{creator.FirstName} {creator.LastName}"
                                      : "—"
                };
            }).ToList();

            return new SuccessDataResult<List<TenderListDto>>(dtos);
        }

        public IDataResult<DashboardSummaryDto> GetDashboardSummary(Guid currentUserGuid)
        {
            var now = DateTime.Now;
            var today = now.Date;
            var start7 = today.AddDays(-6);
            var year = now.Year;

            // 1) Toplam ve tamamlanan ihaleler
            var allTenders = _tenderDal
                .GetList(
                    t => t.IsActive,
                    t => t.Items
                )
                .ToList();

            var totalTenders = allTenders.Count;
            var completedTenders = allTenders.Count(t => t.Status == 4);

            // 2) Ocak (ay=1) raporu
            var januaryList = allTenders
                .Where(t => t.CreatedDate.Year == year && t.CreatedDate.Month == 1)
                .ToList();
            var januaryPeriod = new PeriodCountDto
            {
                Created = januaryList.Count,
                Completed = januaryList.Count(t => t.Status == 4)
            };

            // 3) Son 7 günde yeni tedarikçi
            var suppliersLast7 = _userDal
                .GetList(u => u.CreatedDate.HasValue && u.CreatedDate.Value.Date >= start7 && u.UserRoles.Any(a => a.RoleId == 2))
                .ToList();
            var supGroups = suppliersLast7
                .GroupBy(u => u.CreatedDate!.Value.Date)
                .ToDictionary(g => g.Key, g => g.Count());
            var newSuppliersDays = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = start7.AddDays(i);
                    return new DayCountDto
                    {
                        Day = d.ToString("dddd", new CultureInfo("tr-TR")),
                        Count = supGroups.ContainsKey(d) ? supGroups[d] : 0
                    };
                })
                .ToList();

            // 4) Son 7 günde oluşturulan ihale
            var tendersLast7 = allTenders
                .Where(t => t.CreatedDate.Date >= start7)
                .ToList();
            var tenGroups = tendersLast7
                .GroupBy(t => t.CreatedDate.Date)
                .ToDictionary(g => g.Key, g => g.Count());
            var newTendersDays = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = start7.AddDays(i);
                    return new DayCountDto
                    {
                        Day = d.ToString("dddd", new CultureInfo("tr-TR")),
                        Count = tenGroups.ContainsKey(d) ? tenGroups[d] : 0
                    };
                })
                .ToList();

            // 5) Aylık ihale istatistikleri
            var monthNames = CultureInfo.GetCultureInfo("tr-TR")
                .DateTimeFormat
                .AbbreviatedMonthNames
                .Take(12)
                .ToArray();
            var monthlyStats = Enumerable.Range(1, 12)
                .Select(m => new MonthlyTenderCountDto
                {
                    Month = monthNames[m - 1],
                    Created = allTenders.Count(t => t.CreatedDate.Year == year && t.CreatedDate.Month == m),
                    Completed = allTenders.Count(t => t.CreatedDate.Year == year
                                                  && t.CreatedDate.Month == m
                                                  && t.Status == 4)
                })
                .ToList();

            // 6) Genel ürün sayısı
            var totalProducts = _partDal.GetList().Count;

            // 7) Haftalık talep edilen parça adedi (Tender.Items içerisinden)
            var reqTendersLast7 = allTenders
                .Where(t => t.CreatedDate.Date >= start7)
                .ToList();
            var reqGroups = reqTendersLast7
                .GroupBy(t => t.CreatedDate.Date)
                .ToDictionary(
                    g => g.Key,
                    g => (int)g.SelectMany(t => t.Items).Sum(item => item.Quantity)
                );
            var newRequestedPartsDays = Enumerable.Range(0, 7)
                .Select(i =>
                {
                    var d = start7.AddDays(i);
                    return new DayCountDto
                    {
                        Day = d.ToString("dddd", new CultureInfo("tr-TR")),
                        Count = reqGroups.ContainsKey(d) ? reqGroups[d] : 0
                    };
                })
                .ToList();

            // 8) Toplam talep edilen parça adedi
            var totalRequestedParts = (int)allTenders
                .SelectMany(t => t.Items)
                .Sum(item => item.Quantity);

            // 9) Yıllık talep edilen ürün sayısı (aylık)
            var monthlyRequestedStats = Enumerable.Range(1, 12)
                .Select(m => new MonthlyProductCountDto
                {
                    Month = monthNames[m - 1],
                    Count = (int)allTenders
                        .Where(t => t.CreatedDate.Year == year && t.CreatedDate.Month == m)
                        .SelectMany(t => t.Items)
                        .Sum(item => item.Quantity)
                })
                .ToList();

            // ► Sonuç DTO
            var summary = new DashboardSummaryDto
            {
                TotalTenders = totalTenders,
                CompletedTenders = completedTenders,
                January = januaryPeriod,
                NewSuppliersLast7Days = newSuppliersDays,
                NewTendersLast7Days = newTendersDays,
                MonthlyTenderStats = monthlyStats,
                TotalProducts = totalProducts,
                NewRequestedPartsLast7Days = newRequestedPartsDays,
                TotalRequestedParts = totalRequestedParts,
                MonthlyRequestedProductStats = monthlyRequestedStats
            };

            return new SuccessDataResult<DashboardSummaryDto>(summary);
        }


    }
}
