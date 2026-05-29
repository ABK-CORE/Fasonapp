using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using DataAccess.Concrete.EntityFramework;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;

namespace Business.Concrete
{
    public class TenderManager : ITenderService
    {
        private readonly ITenderDal _tenderDal;
        private readonly ITenderItemDal _itemDal;
        private readonly IPreApprovalApproverDal _preApproverDal;
        private readonly IApprovalRecordDal _approvalRecordDal;
        private readonly ITenderOfferDal _offerDal;
        private readonly IApprovalRuleDal _ruleDal;
        private readonly IApprovalRuleApproverDal _ruleApproverDal;
        private readonly IApprovalRuleService _approvalRuleService;
        private readonly IUserDal _userDal;
        private readonly IMapper _mapper;
        private readonly IAuditLogDal _logDal;
        private readonly IPartDal _partDal;
        private readonly IPurchaseCategoryDal _categoryDal;
        private readonly IContractsDal _contractDal;
        private readonly IContractPartsDal _contractPartsDal;
        private readonly IEmailService _emailService;
        private readonly IPurchaseRequestDal _purchaseRequestDal;


        public TenderManager(
            ITenderDal tenderDal,
            ITenderItemDal itemDal,
            IPreApprovalApproverDal preApproverDal,
            IApprovalRecordDal approvalRecordDal,
            ITenderOfferDal offerDal,
            IApprovalRuleDal ruleDal,
            IApprovalRuleApproverDal ruleApproverDal,
            IUserDal userDal,
            IAuditLogDal logDal,
            IPartDal partDal,
            IPurchaseCategoryDal categoryDal,
            IApprovalRuleService approvalRuleService,
            IContractsDal contractDal,
            IContractPartsDal contractPartsDal,
            IMapper mapper,
            IEmailService emailService,
            IPurchaseRequestDal purchaseRequestDal)
        {
            _tenderDal = tenderDal;
            _itemDal = itemDal;
            _preApproverDal = preApproverDal;
            _approvalRecordDal = approvalRecordDal;
            _offerDal = offerDal;
            _ruleDal = ruleDal;
            _ruleApproverDal = ruleApproverDal;
            _userDal = userDal;
            _logDal = logDal;
            _categoryDal = categoryDal;
            _mapper = mapper;
            _approvalRuleService = approvalRuleService;
            _contractDal = contractDal;
            _contractPartsDal = contractPartsDal;
            _partDal = partDal;
            _emailService = emailService;
            _purchaseRequestDal = purchaseRequestDal;
        }

        public IDataResult<TenderDto> CreateTender(TenderCreateDto dto)
        {
            var purchase = new PurchaseRequest();

            if (dto.PurchaseRequestGuid != null)
            {
                purchase = _purchaseRequestDal.Get(p => p.RequestGuid == dto.PurchaseRequestGuid);
                if (purchase == null)
                {
                    return new ErrorDataResult<TenderDto>(null, "Talep bulunamadı");
                }
                if (purchase.Status != RequestStatus.Approved)
                {
                    return new ErrorDataResult<TenderDto>(null, "Talep durumu uygun değil");
                }
            }

            var category = _categoryDal.Get(c => c.CategoryId == dto.CategoryId);
            if (category == null)
                return new ErrorDataResult<TenderDto>(null, "Seçilen kategori bulunamadı.");

            var tender = new Tender
            {
                TenderGuid = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                CreatedBy = dto.CreatedBy,
                IsOfferBased = dto.IsOfferBased,
                Status = 1,
                CreatedDate = DateTime.Now,
                CategoryId = dto.CategoryId,
                IsActive = true,
                IsPurchaseRequest = purchase.RequestId != 0,
                PurchaseId = purchase.RequestId != 0 ? purchase.RequestId : 0
            };
            _tenderDal.Add(tender);

            foreach (var item in dto.Items)
            {
                _itemDal.Add(new TenderItem
                {
                    TenderGuid = tender.TenderGuid,
                    PartId = item.PartId,
                    Quantity = item.Quantity,
                    Unit = item.Unit,
                    Note = item.Note
                });
            }

            if (dto.IsOfferBased == 0 || dto.IsOfferBased == 2)
            {
                if (dto.Offers == null || !dto.Offers.Any())
                    return new ErrorDataResult<TenderDto>(null, "Personel teklifleri için DTO.Offers listesi boş olamaz.");

                foreach (var group in dto.Offers)
                {
                    foreach (var o in group.Offers)
                    {
                        decimal unitPrice;
                        string currency;
                        decimal exchangeRateToTL;
                        decimal priceInTL;

                        if (o.UseContractPrice)
                        {
                            var now = DateTime.UtcNow;
                            var contracts = _contractDal.GetList(c =>
                                c.IsActive &&
                                c.SupplierGuid == group.supplierId &&
                                c.StartDate <= now &&
                                c.EndDate >= now).ToList();

                            ContractPart? matchedContractPart = null;
                            foreach (var contract in contracts)
                            {
                                var contractPart = _contractPartsDal.Get(p =>
                                    p.ContractId == contract.ContractId &&
                                    p.PartId == o.PartId);
                                if (contractPart != null)
                                {
                                    matchedContractPart = contractPart;
                                    break;
                                }
                            }

                            if (matchedContractPart == null)
                            {
                                return new ErrorDataResult<TenderDto>(null, "Tedarikçinin geçerli sözleşmelerinde bu parça için fiyat bulunamadı.");
                            }

                            unitPrice = matchedContractPart.UnitPrice;
                            currency = matchedContractPart.Currency;

                            exchangeRateToTL = currency == "TL" ? 1 : o.ExchangeRate;
                        }
                        else
                        {
                            unitPrice = o.UnitPrice;
                            currency = o.Currency;
                            exchangeRateToTL = o.ExchangeRate;
                        }

                        // TL cinsinden fiyatı hesapla
                        priceInTL = unitPrice * exchangeRateToTL;

                        if (priceInTL <= 0.0001m)
                            return new ErrorDataResult<TenderDto>(null, "Birim fiyatın TL karşılığı 0’dan büyük olmalı.");

                        _offerDal.Add(new TenderOffer
                        {
                            OfferGuid = Guid.NewGuid(),
                            TenderGuid = tender.TenderGuid,
                            SupplierId = group.supplierId.Value,
                            PartId = o.PartId,

                            // Yeni Alanlar
                            UnitPrice = unitPrice,
                            Currency = currency,
                            ExchangeRateToTL = exchangeRateToTL,
                            PriceInTL = priceInTL,

                            CreatedDate = DateTime.UtcNow,
                            IsAccepted = null,
                            SupplyDay = o.SupplyDay
                        });
                    }
                }
            }

            // 4) Ortak adım: Ön-onaycı kuyruğunu oluştur (DEĞİŞİKLİK YOK)
            var preApprovers = _preApproverDal.GetList().OrderBy(p => p.OrderIndex).ToList();
            if (preApprovers.Any())
            {
                var firstOrderIndex = preApprovers.First().OrderIndex;
                bool hasMultipleApprovers = preApprovers.Count > 1;

                foreach (var approver in preApprovers)
                {
                    var approvalRecord = new ApprovalRecord
                    {
                        TenderGuid = tender.TenderGuid,
                        UserGuid = approver.UserGuid,
                        OrderIndex = approver.OrderIndex,
                        RecordType = ApprovalRecordTypes.PreApproval
                    };

                    if (dto.IsOfferBased == 1 && approver.OrderIndex == firstOrderIndex && approver.UserGuid == dto.CreatedBy)
                    {
                        approvalRecord.IsApproved = true;
                        approvalRecord.ActionDate = DateTime.UtcNow;

                        if (!hasMultipleApprovers)
                        {
                            tender.Status = 2;
                            _tenderDal.Update(tender);
                            _logDal.Add(new AuditLog { /* ... */ });
                        }
                    }
                    _approvalRecordDal.Add(approvalRecord);
                }
            }

            // Sıradaki (ilk) onaycıyı bul ve mail gönder (DEĞİŞİKLİK YOK)
            var nextPending = _approvalRecordDal
                .GetList(r => r.TenderGuid == tender.TenderGuid && r.IsApproved == null)
                .OrderBy(r => r.OrderIndex)
                .FirstOrDefault();

            if (nextPending != null)
            {
                var approver = _userDal.Get(u => u.Guid == nextPending.UserGuid);
                if (approver != null && !string.IsNullOrEmpty(approver.Email))
                {
                    var detailsUrl = $"https://po.abkcore.com/tender/{tender.TenderGuid}";
                    _emailService
                        .SendApprovalPendingNotificationAsync(approver.Email, tender.Title, detailsUrl)
                        .Wait();
                }
            }

            // 5) Audit log (DEĞİŞİKLİK YOK)
            var userDetail = _userDal.Get(u => u.Guid == dto.CreatedBy);
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = dto.CreatedBy,
                Category = "Tender",
                Message = dto.IsOfferBased == 0 || dto.IsOfferBased == 2
                    ? $"Standart ihale oluşturuldu; ön-onaycı kuyruğu başlatıldı. ({userDetail.FirstName} {userDetail.LastName})"
                    : $"Personel teklif bazlı ihale oluşturuldu; teklifler eklendi ve ön-onaycı kuyruğu başlatıldı. ({userDetail.FirstName} {userDetail.LastName})",
                RequestId = tender.TenderId,
                Type = "Info"
            });

            // 6) Dönüş DTO’su (DEĞİŞİKLİK YOK)
            var resultDto = MapTenderDto(tender.TenderGuid);
            var message = "İhale başarıyla oluşturuldu.";

            if (purchase.RequestId != 0)
            {
                purchase.Status = RequestStatus.Completed;
                purchase.IsTransferredToTender = true;
                purchase.TenderId = tender.TenderId;
                _purchaseRequestDal.Update(purchase);
            }

            return new SuccessDataResult<TenderDto>(resultDto, message);
        }

        public IDataResult<List<TenderDto>> GetAllTenders()
        {
            var list = _tenderDal
                .GetList(t => t.IsActive)
                .Select(t => MapTenderDto(t.TenderGuid))
                .ToList();
            return new SuccessDataResult<List<TenderDto>>(list);
        }

        public IDataResult<TenderDto> GetTenderByGuid(Guid tenderGuid)
        {
            var t = _tenderDal.Get(x => x.TenderGuid == tenderGuid && x.IsActive);
            if (t == null)
                return new ErrorDataResult<TenderDto>(null, "İhale bulunamadı veya aktif değil.");

            return new SuccessDataResult<TenderDto>(MapTenderDto(tenderGuid));
        }

        public IResult DeleteTender(Guid tenderGuid, Guid user)
        {
            var t = _tenderDal.Get(x => x.TenderGuid == tenderGuid);
            if (t == null)
                return new Result(false, "İhale bulunamadı.");

            //log
            var userDetail = _userDal.Get(u => u.Guid == user);
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = userDetail.Guid,
                Category = "Tender",
                Message = $"Talep {userDetail.FirstName} {userDetail.LastName} tarafından iptal edildi.",
                RequestId = null,
                Type = "Warning"
            });

            t.IsActive = false;
            _tenderDal.Update(t);
            return new SuccessResult("İhale silindi.");
        }

        public IDataResult<PageResult<TenderDto>> GetTendersPaged(int page, int pageSize, TenderFilterDto? filter)
        {
            var query = _tenderDal.Query(
                t => t.IsActive,
                t => t.Items,
                t => t.Offers,
                t => t.ApprovalRecords,
                t => t.Category
            );

            // Filtre
            if (filter != null)
            {
                if (!string.IsNullOrWhiteSpace(filter.Title))
                {
                    var titleLower = filter.Title.Trim().ToLower();
                    query = query.Where(t => t.Title != null && t.Title.ToLower().Contains(titleLower));
                }
                if (filter.CreatedBy.HasValue)
                    query = query.Where(t => t.CreatedBy == filter.CreatedBy.Value);

                if (filter.Status.HasValue)
                    query = query.Where(t => t.Status == filter.Status.Value);

                if (filter.PartId.HasValue)
                    query = query.Where(t => t.Items.Any(i => i.PartId == filter.PartId.Value));

                if (filter.PendingApprover.HasValue)
                    query = query.Where(t => t.ApprovalRecords.Any(r => r.UserGuid == filter.PendingApprover.Value && r.IsApproved == null));
            }

            var totalCount = query.Count();

            var entities = query
                .OrderByDescending(t => t.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var allPartIds = entities.SelectMany(t => t.Items).Select(i => i.PartId).Distinct().ToList();
            var allUserGuids = entities.Select(t => t.CreatedBy)
                .Union(entities.SelectMany(t => t.ApprovalRecords).Select(r => r.UserGuid))
                .Union(entities.SelectMany(t => t.Offers).Select(o => o.SupplierId))
                .Distinct()
                .ToList();

            var partDict = _partDal.GetList(p => allPartIds.Contains(p.PartId))
                .ToDictionary(p => p.PartId, p => p);

            var userDict = _userDal.GetList(u => allUserGuids.Contains(u.Guid), u => u.FirmInfo)
                .ToDictionary(u => u.Guid, u => u);

            var dtos = entities.Select(t =>
            {
                var creatorName = userDict.TryGetValue(t.CreatedBy, out var creatorUser)
                    ? $"{creatorUser.FirstName} {creatorUser.LastName}"
                    : "—";

                var dto = new TenderDto
                {
                    TenderId = t.TenderId,
                    TenderGuid = t.TenderGuid,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedBy = t.CreatedBy,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    CreatedByName = creatorName,
                    IsOfferBased = t.IsOfferBased,

                    Items = t.Items.Select(i => new TenderItemDto
                    {
                        PartId = i.PartId,
                        Quantity = i.Quantity,
                        Unit = i.Unit,
                        Note = i.Note,
                        PartName = partDict.TryGetValue(i.PartId, out var part) ? part.PartName : "—",
                        Photo = partDict.TryGetValue(i.PartId, out var part2) ? part2.PartPhoto : null,
                        Code = partDict.TryGetValue(i.PartId, out var part3) ? part3.PartCode : null
                    }).ToList(),

                    ApprovalRecords = t.ApprovalRecords
                        .OrderBy(r => r.OrderIndex)
                        .Select(r => new ApprovalRecordDto
                        {
                            UserGuid = r.UserGuid,
                            OrderIndex = r.OrderIndex,
                            IsApproved = r.IsApproved,
                            ActionDate = r.ActionDate,
                            RecordType = (int)r.RecordType,
                            UserName = userDict.TryGetValue(r.UserGuid, out var ru)
                                ? $"{ru.FirstName} {ru.LastName}"
                                : "—"
                        }).ToList(),

                    Offers = t.Offers.Select(o =>
                    {
                        var supplier = userDict.TryGetValue(o.SupplierId, out var su) ? su : null;
                        return new TenderOfferDto
                        {
                            OfferId = o.OfferId,
                            OfferGuid = o.OfferGuid,
                            TenderGuid = o.TenderGuid,
                            SupplierId = o.SupplierId,
                            PartId = o.PartId,
                            Price = o.UnitPrice,
                            CreatedDate = o.CreatedDate,
                            IsAccepted = o.IsAccepted,
                            UnitPrice = o.UnitPrice,
                            Currency = o.Currency,
                            ExchangeRateToTL = o.ExchangeRateToTL,
                            PriceInTL = o.PriceInTL,

                            PartName = partDict.TryGetValue(o.PartId, out var po) ? po.PartName : null,
                            PartCode = partDict.TryGetValue(o.PartId, out var po2) ? po2.PartCode : null,
                            SupplierName = supplier != null ? $"{supplier.FirstName} {supplier.LastName}" : null,
                            SupplierCompany = supplier?.FirmInfo?.CompanyName
                        };
                    }).ToList(),

                    Category = new TenderCategoryDto
                    {
                        CategoryId = t.Category!.CategoryId,
                        Name = t.Category.Name
                    },

                    SelectedSupplierId = t.Offers.FirstOrDefault(o => o.IsAccepted == true)?.SupplierId,
                    SelectedSupplierName = t.Offers.FirstOrDefault(o => o.IsAccepted == true) is var win && win != null
                        && userDict.TryGetValue(win.SupplierId, out var uw)
                        ? $"{uw.FirstName} {uw.LastName}"
                        : null,
                    SelectedSupplierCompany = t.Offers.FirstOrDefault(o => o.IsAccepted == true) is var win2 && win2 != null
                        && userDict.TryGetValue(win2.SupplierId, out var uf)
                        ? uf?.FirmInfo?.CompanyName
                        : null,
                };

                return dto;
            }).ToList();

            return new SuccessDataResult<PageResult<TenderDto>>(new PageResult<TenderDto>
            {
                Items = dtos,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        private TenderDto MapTenderDto(Guid tg)
        {
            // 1) Tender'ı çek
            var tender = _tenderDal.Get(x => x.TenderGuid == tg, x => x.Category);

            // 2) Mapper ile temel alanları al
            var dto = _mapper.Map<TenderDto>(tender);

            try
            {
                dto.IsPurchaseRequest = tender.IsPurchaseRequest;
                dto.PurchaseGuid = tender.IsPurchaseRequest ? _purchaseRequestDal.Get(x => x.RequestId == tender.PurchaseId).RequestGuid : null;
            }
            catch (Exception)
            {
                dto.IsPurchaseRequest = false;
                dto.PurchaseGuid = null;
            }


            // 3) CreatedByName
            var creator = _userDal.Get(u => u.Guid == tender.CreatedBy);
            dto.CreatedByName = creator != null
                ? $"{creator.FirstName} {creator.LastName}"
                : "—";

            // 4) Maddeler
            dto.Items = _itemDal
                .GetList(i => i.TenderGuid == tg)
                .Select(i =>
                {
                    var part = _partDal.Get(p => p.PartId == i.PartId);
                    var dtoItem = _mapper.Map<TenderItemDto>(i);
                    dtoItem.PartName = part?.PartName ?? "—";
                    dtoItem.Photo = part?.PartPhoto;
                    dtoItem.Code = part?.PartCode;
                    dtoItem.Id = i.TenderItemId;
                    return dtoItem;
                })
                .ToList();

            // 5) Onay Kayıtları
            dto.ApprovalRecords = _approvalRecordDal
                .GetList(r => r.TenderGuid == tg)
                .OrderBy(r => r.OrderIndex)
                .Select(r =>
                {
                    var u = _userDal.Get(u => u.Guid == r.UserGuid);
                    return new ApprovalRecordDto
                    {
                        UserGuid = r.UserGuid,
                        UserName = u != null
                                      ? $"{u.FirstName} {u.LastName}"
                                      : "—",
                        OrderIndex = r.OrderIndex,
                        IsApproved = r.IsApproved,
                        ActionDate = r.ActionDate,
                        RecordType = r.RecordType
                    };
                })
                .ToList();

            // ► SÖZLEŞME LOOKUP
            var now = DateTime.UtcNow;
            var supplierIds = _offerDal
                .GetList(o => o.TenderGuid == tg)
                .Select(o => o.SupplierId)
                .Distinct()
                .ToList();

            var contracts = _contractDal.GetList(
                c => supplierIds.Contains(c.SupplierGuid)
                     && c.IsActive
                     && c.StartDate <= now
                     && c.EndDate >= now,
                c => c.Parts
            ).ToList();

            var contractLookup = contracts
                .SelectMany(c => c.Parts.Select(p => new { Contract = c, Part = p }))
                .ToLookup(x => x.Part.PartId);

            // 6) Teklifler (Offers)
            dto.Offers = _offerDal
                .GetList(o => o.TenderGuid == tg)
                .Select(o =>
                {
                    var offerDto = _mapper.Map<TenderOfferDto>(o);
                    offerDto.Price = o.UnitPrice;

                    offerDto.UnitPrice = o.UnitPrice;
                    offerDto.Currency = o.Currency;
                    offerDto.ExchangeRateToTL = o.ExchangeRateToTL;
                    offerDto.PriceInTL = o.PriceInTL;

                    // --- burada supplierId filtresi ekleniyor ---
                    var matches = contractLookup[o.PartId]
                        .Where(x => x.Contract.SupplierGuid == o.SupplierId);

                    if (matches.Any())
                    {
                        var match = matches.First();
                        offerDto.IsInContract = true;
                        offerDto.ContractTitle = match.Contract.Title;
                        offerDto.ContractPrice = match.Part.UnitPrice;
                    }
                    else
                    {
                        offerDto.IsInContract = false;
                        offerDto.ContractTitle = null;
                        offerDto.ContractPrice = null;
                    }

                    var user = _userDal.Get(u => u.Guid == o.SupplierId, u => u.FirmInfo);
                    offerDto.SupplierName = $"{user.FirstName} {user.LastName}";
                    offerDto.SupplierCompany = user.FirmInfo?.CompanyName;
                    offerDto.SupplyDay = o.SupplyDay;

                    return offerDto;
                })
                .ToList();

            // 7) Kazananı ekle
            var winning = dto.Offers.FirstOrDefault(o => o.IsAccepted == true);
            if (winning != null)
            {
                dto.SelectedSupplierId = winning.SupplierId;
                dto.SelectedSupplierName = winning.SupplierName;
                dto.SelectedSupplierCompany = winning.SupplierCompany;
            }

            // 8) Bütçe hesaplaması (aynı)
            if (dto.Category != null)
            {
                var category = _categoryDal.Get(c => c.CategoryId == dto.Category.CategoryId);
                if (category != null)
                {
                    DateTime periodStart = category.LimitPeriod == LimitPeriodType.Monthly
                        ? new DateTime(now.Year, now.Month, 1)
                        : new DateTime(now.Year, 1, 1);

                    var completed = _tenderDal.GetList(
                        t => t.CategoryId == category.CategoryId
                             && t.Status == 4
                             && t.CreatedDate >= periodStart
                             && t.IsActive,
                        t => t.Offers,
                        t => t.Items
                    );

                    decimal usedQty = completed.Sum(t =>
                        t.Offers.Where(o => o.IsAccepted == true)
                                .Sum(o => {
                                    var item = t.Items.First(i => i.PartId == o.PartId);
                                    return o.UnitPrice * item.Quantity;
                                })
                    );

                    dto.PeriodUsedQuantity = usedQty;
                    dto.IsPeriodLimitExceeded = usedQty > category.LimitAmount;
                    dto.PeriodLimitMessage = category.LimitPeriod == LimitPeriodType.Monthly
                        ? $"Bu ayki {category.Name} limiti {category.LimitAmount.ToString("N2", new CultureInfo("tr-TR"))}₺; şu ana kadar {usedQty.ToString("N2", new CultureInfo("tr-TR"))}₺."
                        : $"Bu yıldaki {category.Name} limiti {category.LimitAmount.ToString("N2", new CultureInfo("tr-TR"))}₺; şu ana kadar {usedQty.ToString("N2", new CultureInfo("tr-TR"))}₺.";
                }
            }

            return dto;
        }

        public IDataResult<List<TenderListDto>> GetTendersByStatus(int status)
        {
            var entities = _tenderDal.GetList(t => t.Status == status && t.IsActive);

            var dtos = entities.Select(t => new TenderListDto
            {
                TenderId = t.TenderId,
                TenderGuid = t.TenderGuid,
                Title = t.Title,
                CreatedDate = t.CreatedDate,
                CreatedByName = _userDal.Get(u => u.Guid == t.CreatedBy)?.FirstName + " " + _userDal.Get(u => u.Guid == t.CreatedBy)?.LastName,
                Status = t.Status
            }).ToList();

            return new SuccessDataResult<List<TenderListDto>>(dtos);
        }

        public IResult SubmitOffers(TenderOfferCreateDto dto, Guid currentUserGuid)
        {
            // 1) İhale kontrolü
            var tender = _tenderDal.Get(t => t.TenderGuid.ToString() == dto.TenderGuid);
            if (tender == null)
                return new Result(false, "İhale bulunamadı.");

            if (tender.Status != 2)
                return new Result(false, "Bu ihale için artık teklif veremezsiniz.");

            // 2) Zaten teklif vermiş mi?
            bool alreadyOffered = _offerDal.GetList(o =>
                o.TenderGuid.ToString() == dto.TenderGuid &&
                o.SupplierId == currentUserGuid).Any();
            if (alreadyOffered)
                return new Result(false, "Zaten bu ihaleye teklif verdiniz. Değiştirilemez.");

            // 3) Teklif satırlarını ekle
            foreach (var o in dto.Offers)
            {
                if (o.UnitPrice <= 0)
                    return new Result(false, "Birim fiyat 0’dan büyük olmalı.");

                _offerDal.Add(new TenderOffer
                {
                    OfferGuid = Guid.NewGuid(),
                    TenderGuid = Guid.Parse(dto.TenderGuid),
                    SupplierId = currentUserGuid,
                    PartId = o.PartId,
                    UnitPrice = o.UnitPrice,
                    CreatedDate = DateTime.UtcNow,
                    SupplyDay = o.SupplyDay
                });
            }

            // 4) Log
            var userDetail = _userDal.Get(
                u => u.Guid == currentUserGuid,
                u => u.FirmInfo
                );

            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = currentUserGuid,
                Category = "Tender",
                Message = $"Talep {userDetail.FirstName} {userDetail.LastName} ({userDetail.FirmInfo.CompanyName}) tarafından teklif verildi.",
                RequestId = tender.TenderId,
                Type = "Info"
            });


            var supplierEmail = userDetail.Email;
            if (!string.IsNullOrEmpty(supplierEmail))
            {
               // _emailService.SendOfferReceivedNotificationAsync(supplierEmail, tender.Title).Wait();
            }


            return new SuccessResult("Teklifler başarıyla kaydedildi.");
        }

        public IResult CloseTenderAndStartApproval(Guid tenderGuid, Guid currentUserGuid, Guid selectedSupplierGuid)
        {
            // 1) Tender’ı getir
            var tender = _tenderDal.Get(t => t.TenderGuid == tenderGuid);
            if (tender == null)
                return new Result(false, "İhale bulunamadı.");

            if (!tender.IsActive)
                return new Result(false, "Bu ihale artık aktif değil.");

            // 2) Yalnızca Status = 2’den 3’e geçiş izni
            if (tender.Status != 2)
                return new Result(false, "Bu ihale şu anda kapatılamaz.");

            // 3) Status’u 3 olarak güncelle
            tender.Status = 3;
            _tenderDal.Update(tender);

            // 4) Seçilen tedarikçinin tüm kalem tekliflerini çek
            var items = _itemDal.GetList(i => i.TenderGuid == tenderGuid);
            var offers = _offerDal
                .GetList(o => o.TenderGuid == tenderGuid && o.SupplierId == selectedSupplierGuid)
                .ToList();

            // **4a) Offer’ları IsSelected = true olarak işaretle**
            foreach (var o in offers)
            {
                o.IsAccepted = true;
                _offerDal.Update(o);
            }

            // 4b) Toplam tutarı hesapla (birim fiyat * adet, tüm kalemler)
            decimal totalAmount = items.Sum(item =>
            {
                var offer = offers.FirstOrDefault(o => o.PartId == item.PartId);
                return offer != null
                    ? offer.UnitPrice * item.Quantity
                    : 0m;
            });

            // 5) Onaycıları çek
            var approversResult = _approvalRuleService.GetApproversByAmount(totalAmount);
            if (!approversResult.Success)
                return new Result(false, approversResult.Message);

            var approvers = approversResult.Data;

            // 6) Her onaycı için yeni record ekle
            foreach (var a in approvers)
            {
                _approvalRecordDal.Add(new ApprovalRecord
                {
                    TenderGuid = tender.TenderGuid,
                    UserGuid = a.UserGuid,
                    OrderIndex = a.OrderIndex,
                    RecordType = ApprovalRecordTypes.AmountApproval
                });
            }

            // 8) Audit log
            var currentUser = _userDal.Get(u => u.Guid == currentUserGuid);
            string userName = $"{currentUser.FirstName} {currentUser.LastName}";
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = currentUserGuid,
                Category = "Tender",
                Message = $"{userName} ihaleyi kapattı, {totalAmount:N2} tutar için onaycılar getirildi ve teklifler işaretlendi.",
                RequestId = tender.TenderId,
                Type = "Info"
            });

            var winner = _userDal.Get(u => u.Guid == selectedSupplierGuid);
            if (!string.IsNullOrEmpty(winner?.Email))
            {
                var detailsUrl = $"https://po.abkcore.com/tender/{tender.TenderGuid}";
                //_emailService.SendTenderWinnerNotificationAsync(winner.Email, tender.Title, detailsUrl).Wait();
            }

            return new SuccessResult("İhale kapatıldı, teklifler işaretlendi ve onay süreci başlatıldı.");
        }

        public IDataResult<List<TenderDto>> GetTendersForSupplier(Guid supplierGuid)
        {
            var tenders = _tenderDal.GetList(
                t =>
                    (t.Status == 2 && t.IsActive) || // Aktif ve teklif verilebilir
                    (
                        (t.Status == 3 || t.Status == 4 || t.Status == 5) &&
                        t.Offers.Any(o => o.SupplierId == supplierGuid) // Teklif verdiyse pasif olsa bile gör
                    ),
                t => t.Items,
                t => t.Offers
            );

            var dtoList = tenders.Select(t =>
            {
                var creator = _userDal.Get(u => u.Guid == t.CreatedBy);
                var creatorName = creator != null
                    ? $"{creator.FirstName} {creator.LastName}"
                    : "—";

                // Katılım durumu
                var participated = t.Offers.Any(o => o.SupplierId == supplierGuid);

                // Kazanma durumu
                var won = t.Offers.Any(o => o.SupplierId == supplierGuid && o.IsAccepted == true);

                // Kazanan teklif
                var winnerOffer = t.Offers.FirstOrDefault(o => o.IsAccepted == true);
                var winner = winnerOffer != null
                    ? _userDal.Get(u => u.Guid == winnerOffer.SupplierId, u => u.FirmInfo)
                    : null;

                var currentSupplier = _userDal.Get(u => u.Guid == supplierGuid, u => u.FirmInfo);

                return new TenderDto
                {
                    TenderId = t.TenderId,
                    TenderGuid = t.TenderGuid,
                    Title = t.Title,
                    Description = t.Description,
                    CreatedBy = t.CreatedBy,
                    CreatedDate = t.CreatedDate,
                    Status = t.Status,
                    CreatedByName = creatorName,
                    IsOfferBased = t.IsOfferBased,
                    IsParticipated = participated,
                    IsWon = won,

                    SelectedSupplierId = winnerOffer?.SupplierId,
                    SelectedSupplierName = winner != null ? $"{winner.FirstName} {winner.LastName}" : null,
                    SelectedSupplierCompany = winner?.FirmInfo?.CompanyName,

                    Items = t.Items.Select(i =>
                    {
                        var part = _partDal.Get(p => p.PartId == i.PartId);
                        return new TenderItemDto
                        {
                            PartId = i.PartId,
                            Quantity = i.Quantity,
                            Unit = i.Unit,
                            Note = i.Note,
                            PartName = part?.PartName ?? "—",
                            Photo = part?.PartPhoto,
                            Code = part?.PartCode
                        };
                    }).ToList()
                };
            }).ToList();

            return new SuccessDataResult<List<TenderDto>>(dtoList);
        }

        public IDataResult<List<usp_GetStoklarByStokKoduDto>> GetStoklarByStokKodu(string partCode)
        {
            var result = _tenderDal.GetByStokKodu(partCode);
            return result;
        }

        public IDataResult<bool> UpdateTenderCount(UpdateTenderCountDto dto)
        {
            var tender = _tenderDal.Get(t => t.TenderGuid == dto.Guid, t => t.Items);
            if (tender == null)
            {
                return new ErrorDataResult<bool>(false, "Satın Alma Talebi bulunamadı.");
            }

            var item = tender.Items.FirstOrDefault(i => i.PartId == dto.PartId);
            if (item == null)
            {
                return new ErrorDataResult<bool>(false, "Satın Alma Talebi kalemi bulunamadı.");
            }

            item.Quantity = dto.Quantity;

            _itemDal.Update(item);

            return new SuccessDataResult<bool>(true, "Satın Alma Talebi kalemi güncellendi.");
        }

        public IResult CompletedTender(TenderCompletedDto dto, Guid CurrentUserGuid)
        {
            var tender = _tenderDal.Get(
                t => t.TenderGuid == dto.TenderGuid,
                t => t.Items
            );

            if (tender == null)
            {
                return new Result(false, "Satın alma bulunamadı.");
            }
            if (tender.Status != 4)
            {
                return new Result(false, "Bu satın alma tamamlanamaz.");
            }

            tender.DeliveryNoteNumber = dto.DeliveryNoteNumber;
            tender.InvoiceNumber = dto.InvoiceNumber;
            tender.DeliveryNoteDate = dto.DeliveryNoteDate;
            tender.InvoiceDate = dto.InvoiceDate;
            tender.Status = 6;
            _tenderDal.Update(tender);

            foreach (var item in tender.Items)
            {
                item.DeliveredProduct = dto?.Products?
                    .FirstOrDefault(p => p.ProductId == item.TenderItemId)
                    ?.Quantity ?? 0;

                _itemDal.Update(item);
            }

            // Log
            var userDetail = _userDal.Get(u => u.Guid == CurrentUserGuid);
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = CurrentUserGuid,
                Category = "Tender",
                Message = $"Satın alma {userDetail.FirstName} {userDetail.LastName} tarafından tamamlandı.",
                RequestId = tender.TenderId,
                Type = "Info"
            });
            return new SuccessResult("Satın alma başarıyla tamamlandı.");
        }

        public IResult UpdateOfferPrice(UpdateOfferPriceDto dto, Guid currentUserGuid)
        {
            // 1. Teklifi bul
            var offer = _offerDal.Get(o => o.OfferGuid == dto.OfferGuid);
            if (offer == null)
            {
                return new Result(false, "Güncellenmek istenen teklif bulunamadı.");
            }

            // 2. Ana ihaleyi bul ve durumunu kontrol et
            var tender = _tenderDal.Get(t => t.TenderGuid == offer.TenderGuid);
            if (tender == null)
            {
                return new Result(false, "İhale bulunamadı.");
            }

            // Fiyat güncellemesine sadece belirli durumlarda izin ver (Örn: Teklif Onayına Sunuldu)
            if (tender.Status != 3)
            {
                return new Result(false, "Bu aşamadaki bir ihalenin teklif fiyatı güncellenemez.");
            }

            // 3. Fiyatın geçerliliğini kontrol et
            if (dto.NewUnitPrice <= 0)
            {
                return new Result(false, "Birim fiyat 0'dan büyük olmalıdır.");
            }

            // 4. Fiyatı güncelle
            var oldPrice = offer.UnitPrice;
            var oldPriceInTl = offer.PriceInTL;

            offer.UnitPrice = dto.NewUnitPrice;
            // TL karşılığını da yeniden hesapla
            offer.PriceInTL = dto.NewUnitPrice * offer.ExchangeRateToTL;

            _offerDal.Update(offer);

            // 5. Audit log kaydı oluştur
            var userDetail = _userDal.Get(u => u.Guid == currentUserGuid);
            var userName = $"{userDetail.FirstName} {userDetail.LastName}";

            var part = _partDal.Get(p => p.PartId == offer.PartId);
            var supplier = _userDal.Get(u => u.Guid == offer.SupplierId, u => u.FirmInfo);

            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = currentUserGuid,
                Category = "Tender",
                Message = $"{userName} tarafından '{tender.Title}' ihalesinde, '{supplier.FirmInfo.CompanyName}' tedarikçisinin '{part.PartName}' ürünü için verdiği teklif fiyatı {oldPrice:N2} {offer.Currency}'den {offer.UnitPrice:N2} {offer.Currency}'e güncellendi.",
                RequestId = tender.TenderId,
                Type = "Update"
            });

            return new SuccessResult("Teklif birim fiyatı başarıyla güncellendi.");
        }

        public IResult UpdateOfferSupplyDay(UpdateOfferSupplyDayDto dto, Guid currentUserGuid)
        {
            // 1. Teklifi bul
            var offer = _offerDal.Get(o => o.OfferGuid == dto.OfferGuid);
            if (offer == null)
            {
                return new Result(false, "Güncellenmek istenen teklif bulunamadı.");
            }

            // 2. Ana ihaleyi bul ve durumunu kontrol et
            var tender = _tenderDal.Get(t => t.TenderGuid == offer.TenderGuid);
            if (tender == null)
            {
                return new Result(false, "İhale bulunamadı.");
            }

            // Güncellemeye sadece belirli durumlarda izin ver (Örn: Teklif Onayına Sunuldu)
            if (tender.Status != 3)
            {
                return new Result(false, "Bu aşamadaki bir ihalenin teslim süresi güncellenemez.");
            }

            // 3. Teslim gününün geçerliliğini kontrol et (negatif olamaz)
            if (dto.NewSupplyDay < 0)
            {
                return new Result(false, "Teslim süresi 0'dan küçük olamaz.");
            }

            // 4. Teslim süresini güncelle
            var oldSupplyDay = offer.SupplyDay;
            offer.SupplyDay = dto.NewSupplyDay;
            _offerDal.Update(offer);

            // 5. Audit log kaydı oluştur
            var userDetail = _userDal.Get(u => u.Guid == currentUserGuid);
            var userName = $"{userDetail.FirstName} {userDetail.LastName}";

            var part = _partDal.Get(p => p.PartId == offer.PartId);
            var supplier = _userDal.Get(u => u.Guid == offer.SupplierId, u => u.FirmInfo);

            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = currentUserGuid,
                Category = "Tender",
                Message = $"{userName} tarafından '{tender.Title}' ihalesinde, '{supplier.FirmInfo.CompanyName}' tedarikçisinin '{part.PartName}' ürünü için verdiği teslim süresi {oldSupplyDay} günden {offer.SupplyDay} güne güncellendi.",
                RequestId = tender.TenderId,
                Type = "Update"
            });

            return new SuccessResult("Teklif teslim süresi başarıyla güncellendi.");
        }
    }
}
