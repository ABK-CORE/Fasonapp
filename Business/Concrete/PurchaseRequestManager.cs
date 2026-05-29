using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enum;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Business.Concrete
{
    public class PurchaseRequestManager : IPurchaseRequestService
    {
        private readonly IPurchaseRequestDal _requestDal;
        private readonly IPurchaseRequestItemDal _itemDal;
        private readonly IRequestApprovalRecordDal _recordDal;
        private readonly IDepartmentUserDal _deptUserDal;
        private readonly IDepartmentApprovalProcessService _processService;
        private readonly IPartDal _partDal;
        private readonly IUserDal _userDal;
        private readonly ITenderService _tenderService;
        private readonly IUserRoleDal _userRoleDal;
        private readonly IEmailService _emailService;

        public PurchaseRequestManager(
            IPurchaseRequestDal requestDal,
            IPurchaseRequestItemDal itemDal,
            IRequestApprovalRecordDal recordDal,
            IDepartmentUserDal deptUserDal,
            IDepartmentApprovalProcessService processService,
            IPartDal partDal,
            IUserDal userDal,
            IUserRoleDal userRoleDal,
            ITenderService tenderService,
            IEmailService emailService)
        {
            _requestDal = requestDal;
            _itemDal = itemDal;
            _recordDal = recordDal;
            _deptUserDal = deptUserDal;
            _processService = processService;
            _partDal = partDal;
            _userDal = userDal;
            _userRoleDal = userRoleDal;
            _tenderService = tenderService;
            _emailService = emailService;
        }

        public IDataResult<List<PurchaseRequestDto>> GetAll(Guid currentUserGuid)
        {
            // 1) Kullanıcının rollerini oku
            var userRoles = _userRoleDal
                .GetList(ur => ur.UserGuid == currentUserGuid, ur => ur.Role)
                .Select(ur => ur.Role!.RoleName)
                .ToList();

            bool isTenderMgmt = userRoles.Contains("TenderManagement");

            // 2) Kullanıcının departman bilgisi ve managerLevel’ı
            var deptUsers = _deptUserDal
                .GetList(du => du.UserGuid == currentUserGuid)
                .ToList();

            var deptIds = deptUsers.Select(du => du.DepartmentId).ToList();
            bool isManager = deptUsers.Any(du => du.ManagerLevel.HasValue);

            // 3) Filtre koşulunu belirle
            Expression<Func<PurchaseRequest, bool>> filter;
            if (isTenderMgmt)
            {
                // TenderManagement rolü → tüm talepler
                filter = pr => true;
            }
            else if (isManager)
            {
                // Departman yöneticisi → kendi departmanındaki talepler
                filter = pr => deptIds.Contains(pr.DepartmentId);
            }
            else
            {
                // Normal kullanıcı → sadece kendisinin oluşturdukları
                filter = pr => pr.CreatedBy == currentUserGuid;
            }

            // 4) Veriyi eager-load ile getir ve DTO’ya map et
            var requests = _requestDal.GetList(
                filter: filter,
                eager: q => q
                    .Include(pr => pr.Category)
                    .Include(pr => pr.Items).ThenInclude(i => i.Part)
                    .Include(pr => pr.ApprovalRecords)
            );

            var dtos = requests
                .Select(MapToDto)
                .OrderByDescending(x => x.RequestId)
                .ToList();

            return new SuccessDataResult<List<PurchaseRequestDto>>(dtos);
        }

        public IDataResult<PurchaseRequestDto> GetByGuid(Guid requestGuid, Guid currentUserGuid)
        {
            // 1) Entity’i eager load ile al
            var pr = _requestDal.Get(
              pr => pr.RequestGuid == requestGuid,
              q => q
                .Include(pr => pr.Category)
                .Include(pr => pr.Items).ThenInclude(i => i.Part)
                .Include(pr => pr.ApprovalRecords)
            );
            if (pr == null)
                return new ErrorDataResult<PurchaseRequestDto>(null, "Talep bulunamadı.");

            // 2) DTO’ya çevir
            var dto = MapToDto(pr);

            // 3) İlk bekleyen onay adımını bul
            var nextRecord = pr.ApprovalRecords
                .Where(r => r.IsApproved == null)
                .OrderBy(r => r.OrderIndex)
                .FirstOrDefault();

            bool isNext = false;
            if (nextRecord != null)
            {
                if (nextRecord.StepType == StepType.User)
                {
                    // direkt atanan user ise
                    isNext = nextRecord.UserGuid == currentUserGuid;
                }
                else if (nextRecord.StepType == StepType.ManagerLevel && nextRecord.ManagerLevel.HasValue)
                {
                    // aynı departmandaki ilgili seviyedeki aktif kullanıcılar arasında mı?
                    isNext = _deptUserDal.GetList(du =>
                            du.DepartmentId == pr.DepartmentId
                         && du.ManagerLevel == nextRecord.ManagerLevel.Value
                         && du.UserGuid == currentUserGuid
                        ).Any();
                }

                dto.NextApprover = GetApproverInfo(pr.DepartmentId, nextRecord);
            }


            dto.IsNextApprover = isNext;
            return new SuccessDataResult<PurchaseRequestDto>(dto);
        }

        private UserDto GetApproverInfo(int departmentId, RequestApprovalRecord record)
        {
            // 1) User adımıysa direkt UserGuid üzerinden
            if (record.StepType == StepType.User && record.UserGuid.HasValue)
            {
                var user = _userDal.Get(u => u.Guid == record.UserGuid.Value);
                return new UserDto
                {
                    UserGuid = user.Guid,
                    FirstName = user.FirstName!,
                    LastName = user.LastName!
                };
            }

            // 2) ManagerLevel adımıysa, ilgili seviyedeki aktif kullanıcı
            if (record.StepType == StepType.ManagerLevel && record.ManagerLevel.HasValue)
            {
                var managerDu = _deptUserDal
                    .GetList(
                        du => du.DepartmentId == departmentId
                           && du.ManagerLevel == record.ManagerLevel.Value
                           && du.User.IsActive,
                        du => du.User
                    )
                    .FirstOrDefault();

                if (managerDu != null)
                {
                    var mgr = managerDu.User;
                    return new UserDto
                    {
                        UserGuid = mgr.Guid,
                        FirstName = mgr.FirstName!,
                        LastName = mgr.LastName!
                    };
                }

                // Yönetici bulunamadıysa fallback label
                var level = record.ManagerLevel.Value;
                return new UserDto
                {
                    FirstName = $"{level}. Üst Yönetici Onayı",
                    LastName = "(Yönetici Bilgisine Ulaşılamadı)"
                };
            }

            // 3) Diğer durumlar için genel fallback
            return new UserDto
            {
                FirstName = "Onay Süreci",
                LastName = "(Atanmamış)"
            };
        }

        public IResult Create(PurchaseRequestCreateDto dto, Guid createdBy)
        {
            // 1) Kullanıcının departmanını bul
            var du = _deptUserDal
                .GetList(x => x.UserGuid == createdBy, x => x.Department)
                .FirstOrDefault(x => x.Department != null && x.Department.IsActive);

            if (du == null)
                return new Result(false, "Kullanıcının departmanı bulunamadı.");

            if (du.Department == null)
            {
                return new Result(false, "Kullanıcının departmanı geçersiz veya pasif durumda.");
            }

            // 2) Satır doğrulamaları
            foreach (var line in dto.Items)
            {
                if (line.Quantity < 1)
                    return new Result(false, "Miktar 1'den az olamaz.");

                var part = _partDal.Get(p => p.PartId == line.PartId && p.IsActive);
                if (part == null)
                    return new Result(false, $"Geçersiz parça: {line.PartId}");
            }

            // 3) Talep başlığıyla birlikte kaydet
            var pr = new PurchaseRequest
            {
                DepartmentId = du.DepartmentId,
                CreatedBy = createdBy,
                Title = dto.Title,
                Description = dto.Description,
                RequiredDate = dto.RequiredDate,
                Status = RequestStatus.PendingApproval,
                CategoryId = dto.CategoryId
            };
            _requestDal.Add(pr);

            // 4) Her satırı ekle
            foreach (var line in dto.Items)
            {
                _itemDal.Add(new PurchaseRequestItem
                {
                    RequestGuid = pr.RequestGuid,
                    PartId = line.PartId,
                    Quantity = line.Quantity,
                    Unit = line.Unit
                });
            }

            // 5) Onay adımlarını bootstrap et
            var stepsResult = _processService.GetSteps(du.Department.Guid);
            if (!stepsResult.Success)
                return new Result(false, $"Onay akışı alınamadı: {stepsResult.Message}");

            foreach (var s in stepsResult.Data.OrderBy(x => x.OrderIndex))
            {
                _recordDal.Add(new RequestApprovalRecord
                {
                    RequestGuid = pr.RequestGuid,
                    OrderIndex = s.OrderIndex,
                    StepType = s.StepType,
                    UserGuid = s.StepType == StepType.User
                                    ? s.UserGuid
                                    : null,
                    ManagerLevel = s.StepType == StepType.ManagerLevel
                                    ? s.ManagerLevel
                                    : null
                });
            }

            var nextRecord = _recordDal.GetList(r =>
        r.RequestGuid == pr.RequestGuid && r.IsApproved == null)
    .OrderBy(r => r.OrderIndex)
    .FirstOrDefault();

            if (nextRecord != null && nextRecord.UserGuid.HasValue)
            {
                var approver = _userDal.Get(u => u.Guid == nextRecord.UserGuid.Value);
                var detailsUrl = $"https://po.abkcore.com/purchase-requests/{pr.RequestGuid}";
                // Fire-and-forget
                _ = _emailService.SendPurchaseRequestCreatedNotificationAsync(
                    approver.Email!,
                    pr.Title,
                    $"{approver.FirstName} {approver.LastName}",
                    detailsUrl
                );
            }

            return new SuccessResult("Talep oluşturuldu.");
        }

        public IResult ApproveStep(Guid requestGuid, Guid approverGuid, bool isApproved)
        {
            // 1) Talebi al ve departmanId’yi çıkar
            var prEntity = _requestDal.Get(pr => pr.RequestGuid == requestGuid);
            if (prEntity == null)
                return new Result(false, "Talep bulunamadı.");

            var deptId = prEntity.DepartmentId;

            // 2) Bekleyen onay adımlarını sırala
            var pending = _recordDal.GetList(r =>
                    r.RequestGuid == requestGuid &&
                    r.IsApproved == null)
                .OrderBy(r => r.OrderIndex)
                .ToList();

            var record = pending.FirstOrDefault();
            if (record == null)
                return new Result(false, "Onay adımı bulunamadı veya süreç tamamlandı.");

            // 3) Bu adımdaki gerçek onaycıyı bul
            Guid? expectedApprover = null;

            if (record.StepType == StepType.User)
            {
                expectedApprover = record.UserGuid;
            }
            else if (record.StepType == StepType.ManagerLevel)
            {
                var du = _deptUserDal.GetList(
                    du => du.DepartmentId == deptId
                       && du.ManagerLevel == record.ManagerLevel
                       && du.User.IsActive,
                    du => du.User
                ).FirstOrDefault();

                if (du != null)
                    expectedApprover = du.UserGuid;
            }

            // 4) Yetki kontrolü
            if (expectedApprover.HasValue && expectedApprover.Value != approverGuid)
                return new Result(false, "Bu adımı onaylamaya yetkiniz yok.");

            // 5) Onay/red bilgisi
            if (!expectedApprover.HasValue)
            {
                record.IsApproved = true;
                record.ActionDate = DateTime.UtcNow;
            }
            else
            {
                record.IsApproved = isApproved;
                record.ActionDate = DateTime.UtcNow;
                record.UserGuid = approverGuid;
            }
            _recordDal.Update(record);

            // 6) Reddedildiyse süreci sonlandır
            if (isApproved == false && expectedApprover.HasValue)
            {
                prEntity.Status = RequestStatus.Rejected;
                _requestDal.Update(prEntity);

                // Bildirim: Talep reddedildi
                var creator = _userDal.Get(u => u.Guid == prEntity.CreatedBy);
                _ = _emailService.SendApprovalRejectedNotificationAsync(
                    creator.Email!,
                    prEntity.Title,
                    $"https://po.abkcore.com/purchase-requests/{requestGuid}"
                );

                return new SuccessResult("Talep reddedildi.");
            }

            // 7) Bir sonraki adımı al
            var next = _recordDal.GetList(r =>
                    r.RequestGuid == requestGuid &&
                    r.IsApproved == null)
                .OrderBy(r => r.OrderIndex)
                .FirstOrDefault();

            if (next == null)
            {
                // Hepsi tamamlandı → talep onaylandı
                prEntity.Status = RequestStatus.Approved;
                _requestDal.Update(prEntity);

                // Bildirim: Satın almacı onayladı
                var creator = _userDal.Get(u => u.Guid == prEntity.CreatedBy);
                _ = _emailService.SendPurchaseRequestProcurementApprovedNotificationAsync(
                    creator.Email!,
                    prEntity.Title,
                    $"https://po.abkcore.com/purchase-requests/{requestGuid}"
                );

                return new SuccessResult("Tüm onay adımları tamamlandı, talep onaylandı.");
            }

            // 8) Pasif/eksik adımları otomatik geç
            if (next.StepType == StepType.ManagerLevel)
            {
                var duNext = _deptUserDal.GetList(
                    du => du.DepartmentId == deptId
                       && du.ManagerLevel == next.ManagerLevel
                       && du.User.IsActive,
                    du => du.User
                ).FirstOrDefault();

                if (duNext == null)
                    return ApproveStep(requestGuid, Guid.Empty, true);
            }
            else if (next.StepType == StepType.User && !next.UserGuid.HasValue)
            {
                return ApproveStep(requestGuid, Guid.Empty, true);
            }

            // Bildirim: Bir sonraki onaycıya haber ver
            if (next.UserGuid.HasValue)
            {
                var nextUser = _userDal.Get(u => u.Guid == next.UserGuid.Value);
                _ = _emailService.SendPurchaseRequestPendingApprovalNotificationAsync(
                    nextUser.Email!,
                    prEntity.Title,
                    $"https://po.abkcore.com/purchase-requests/{requestGuid}"
                );
            }

            return new SuccessResult("Onaylandı, bir sonraki onaycıyı bekliyor.");
        }

        private PurchaseRequestDto MapToDto(PurchaseRequest pr)
        {
            // creator
            var creator = _userDal.Get(u => u.Guid == pr.CreatedBy);
            var creatorDto = new UserDto
            {
                UserGuid = creator.Guid,
                FirstName = creator.FirstName!,
                LastName = creator.LastName!
            };

            // items
            var items = pr.Items.Select(i => {
                var p = i.Part;
                return new PurchaseRequestItemDto
                {
                    PartId = i.PartId,
                    Quantity = i.Quantity,
                    PartCode = p.PartCode,
                    PartName = p.PartName,
                    PartDescription = p.Description,
                    Unit = i.Unit
                };
            }).ToList();

            // approvals
            var approvals = pr.ApprovalRecords
                              .OrderBy(r => r.OrderIndex)
                              .Select(r => new RequestApprovalRecordDto
                              {
                                  OrderIndex = r.OrderIndex,
                                  StepType = r.StepType,
                                  ManagerLevel = r.ManagerLevel,
                                  IsApproved = r.IsApproved,
                                  ActionDate = r.ActionDate,
                                  Approver = GetApproverInfo(pr.DepartmentId, r)
                              })
                              .ToList();

            return new PurchaseRequestDto
            {
                RequestId = pr.RequestId,
                RequestGuid = pr.RequestGuid,
                DepartmentId = pr.DepartmentId,
                CreatedByUser = creatorDto,
                CreatedDate = pr.CreatedDate,
                RequiredDate = pr.RequiredDate,
                Title = pr.Title,
                Description = pr.Description,
                Status = pr.Status,
                CategoryId = pr.Category.CategoryId,
                CategoryName = pr.Category.Name,
                Items = items,
                ApprovalRecords = approvals
            };
        }

        public IResult Cancel(Guid requestGuid, Guid canceledBy)
        {
            var pr = _requestDal.Get(x => x.RequestGuid == requestGuid);
            if (pr == null)
                return new Result(false, "Talep bulunamadı.");

            if (pr.Status == RequestStatus.Canceled)
                return new Result(false, "Talep zaten iptal edilmiş.");

            if (pr.Status == RequestStatus.Completed)
            {
                return new Result(false, "Tamamlanmış talepler iptal edilemez.");
            }

            pr.Status = (RequestStatus)(-1);
            _requestDal.Update(pr);

            return new SuccessResult("Talep iptal edildi.");
        }

        public IResult TransferToTender(Guid requestGuid, Guid completedBy,int isOfferBased, List<TenderCreateOfferDto>? supplierOffers)
        {
            // 1) Talebi, kalemleri ve kategori yükle
            var pr = _requestDal.Get(
                x => x.RequestGuid == requestGuid,
                x => x.Items,
                x => x.Category
            );
            if (pr == null)
                return new Result(false, "Talep bulunamadı.");

            // 2) Bekleyen onay var mı?
            var hasPending = _recordDal.GetList(r =>
                    r.RequestGuid == requestGuid && r.IsApproved == null)
                .Any();
            if (hasPending)
                return new Result(false, "Önce bekleyen onay adımlarını tamamlayın.");

            // 3) Bir kere aktarılmış mı?
            if (pr.IsTransferredToTender)
                return new Result(false, "Bu talep zaten tender’a aktarılmış.");

            // 4) Eğer IsOfferBased = 0 veya 2 ise supplierOffers zorunlu
            if ((isOfferBased == 0 || isOfferBased == 2)
                && (supplierOffers == null || !supplierOffers.Any()))
            {
                return new Result(false,
                    "Standart/hizmet bazlı ihale için supplierOffers listesi boş olamaz.");
            }

            // 5) TenderCreateDto’yu doldur
            var tenderDto = new TenderCreateDto
            {
                Title = pr.Title,
                Description = pr.Description,
                CreatedBy = completedBy,
                IsOfferBased = isOfferBased,
                CategoryId = pr.CategoryId,
                Items = pr.Items.Select(i => new TenderItemCreateDto
                {
                    PartId = i.PartId,
                    Quantity = i.Quantity,
                    Unit = i.Unit,
                    Note = null
                }).ToList(),
                Offers = new List<TenderCreateOfferDto>()
            };

            // 6) Eğer teklif bilgisi gelmişse aktar
            if (isOfferBased == 0 || isOfferBased == 2)
            {
                tenderDto.Offers = supplierOffers!
                    .Select(so => new TenderCreateOfferDto
                    {   
                        supplierId = so.supplierId,
                        Offers = so.Offers
                    })
                    .ToList();
            }

            // 7) Çağır
            var result = _tenderService.CreateTender(tenderDto);
            if (!result.Success)
                return new Result(false, $"Tender’a aktarılamadı: {result.Message}");

            // 8) İşaretle
            pr.IsTransferredToTender = true;
            _requestDal.Update(pr);

            var creator = _userDal.Get(u => u.Guid == pr.CreatedBy);
            var detailsUrl = $"https://po.abkcore.com/purchase-requests/{requestGuid}";
            _ = _emailService.SendPurchaseRequestProcessedNotificationAsync(
                creator.Email!, pr.Title, detailsUrl
            );

            return new SuccessResult("Talep başarıyla tender’a aktarıldı.");
        }

        public IResult RejectionbyBuyer(Guid requestGuid, Guid currentByGuid)
        {
            // 1) Talebi al
            var pr = _requestDal.Get(x => x.RequestGuid == requestGuid);
            if (pr == null)
                return new Result(false, "Talep bulunamadı.");
            // 2) Durumu kontrol et
            if (pr.Status != RequestStatus.Approved)
                return new Result(false, "Sadece onaylanan talepler reddedilebilir.");
            // 3) Reddet
            pr.Status = RequestStatus.RejectionbyBuyer;
            _requestDal.Update(pr);
            // 4) Onay kayıtlarını güncelle
            var records = _recordDal.GetList(r => r.RequestGuid == requestGuid);
            foreach (var record in records)
            {
                record.IsApproved = false;
                record.ActionDate = DateTime.UtcNow;
                record.UserGuid = currentByGuid;
                _recordDal.Update(record);
            }
            // 5) Bildirim gönder
            var creator = _userDal.Get(u => u.Guid == pr.CreatedBy);
            _ = _emailService.SendPurchaseRequestRejectedNotificationAsync(
                creator.Email!, pr.Title, $"https://po.abkcore.com/purchase-requests/{requestGuid}"
            );
            return new SuccessResult("Talep reddedildi.");
        }

    }
}
