using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;

namespace Business.Concrete
{
    public class ApprovalManager : IApprovalService
    {
        private readonly IApprovalRecordDal _recordDal;
        private readonly ITenderDal _tenderDal;
        private readonly IAuditLogDal _logDal;
        private readonly ITenderService _tenderService;
        private readonly IMapper _mapper;
        private readonly IUserDal _userDal;
        private readonly IEmailService _emailService;

        public ApprovalManager(
            IApprovalRecordDal recordDal,
            ITenderDal tenderDal,
            IAuditLogDal logDal,
            ITenderService tenderService,
            IUserDal userDal,
            IMapper mapper,
            IEmailService emailService)
        {
            _recordDal = recordDal;
            _tenderDal = tenderDal;
            _tenderService = tenderService;
            _logDal = logDal;
            _userDal = userDal;
            _mapper = mapper;
            _emailService = emailService;
        }

        //Sadece Ön Onay
        public IResult ActOnApproval(ApprovalActionDto action)
        {
            // 1) İlgili tender kaydı
            var tender = _tenderDal.Get(x => x.TenderGuid == action.TenderGuid);
            if (tender == null)
                return new Result(false, "İlgili ihale kaydı bulunamadı.");

            // 2) Tüm bekleyen onaycılar
            var pendingApprovals = _recordDal.GetList(r =>
                r.TenderGuid == action.TenderGuid &&
                r.IsApproved == null
            ).ToList();

            // 3) Sıradaki (ilk) onaycıyı al
            var nextApproval = pendingApprovals.OrderBy(r => r.OrderIndex).FirstOrDefault();
            if (nextApproval == null)
                return new Result(false, "Bekleyen onaycı bulunamadı.");

            // 4) Bu kullanıcı sıradaki onaycı değilse işlem yapılmasın
            if (nextApproval.UserGuid != action.UserGuid)
                return new Result(false, "Bu kullanıcı sıradaki onaycı değildir. Onaylama yetkisi yok.");

            // 5) Kaydı güncelle
            nextApproval.IsApproved = action.IsApproved;
            nextApproval.ActionDate = DateTime.UtcNow;
            _recordDal.Update(nextApproval);

            var userDetail = _userDal.Get(u => u.Guid == action.UserGuid);

            // 6) Log ekle
            var verb = action.IsApproved ? "onayladı" : "reddetti";
            _logDal.Add(new AuditLog
            {
                UserGuid = action.UserGuid,
                Timestamp = DateTime.UtcNow,
                Category = "Tender",
                Message = $"Talebi {userDetail.FirstName} {userDetail.LastName} {verb}.",
                RequestId = tender.TenderId,
                Type = action.IsApproved ? "Success" : "Danger"
            });

            // 7) Durumu güncelle
            if (action.IsApproved)
            {
                var stillPending = _recordDal.GetList(r =>
                    r.TenderGuid == action.TenderGuid &&
                    r.IsApproved == null
                ).OrderBy(r => r.OrderIndex).FirstOrDefault();

                if (stillPending == null)
                {
                    //Ödeme onayı bittiyse süreci bitir
                    if (tender.Status == 3)
                    {
                        tender.Status = 4;
                    }
                    else
                    {
                        //Teklif olarak oluşturulduysa
                        if (tender.IsOfferBased == 0 || tender.IsOfferBased == 2)
                        {
                            tender.Status = 2;
                            _tenderDal.Update(tender);

                            _tenderService.CloseTenderAndStartApproval(tender.TenderGuid, action.UserGuid, action.selectedSupplierId.Value);

                            tender.Status = 3; // Ön onay bitti → Total tutar onayına geçilebilir
                        }
                        else
                        {
                            tender.Status = 2; // Tüm onaylar bitti → Teklif onaya sunulabilir
                        }
                    }

                    _tenderDal.Update(tender);                    
                }
                else
                {
                    // Daha onaylanmamış bir kaydınız var, ona mail gönder
                    var nextUser = _userDal.Get(u => u.Guid == stillPending.UserGuid);
                    if (!string.IsNullOrEmpty(nextUser?.Email))
                    {
                        var detailsUrl = $"https://po.abkcore.com/tender/{tender.TenderGuid}";
                        _emailService
                            .SendApprovalPendingNotificationAsync(
                                nextUser.Email,
                                tender.Title,
                                detailsUrl
                            )
                            .Wait();
                    }
                }
            }
            else
            {
                // 5 = Reddedildi
                tender.Status = 5;
                _tenderDal.Update(tender);

                // → İhale sahibine (oluşturan) e‑posta bildirimi
                var creator = _userDal.Get(u => u.Guid == tender.CreatedBy);
                if (!string.IsNullOrEmpty(creator?.Email))
                {
                    var detailsUrl = $"https://po.abkcore.com/tender/{tender.TenderGuid}";

                    // IEmailService'e bu metodu eklemiş olduğunuzu varsayıyoruz
                    _emailService
                        .SendApprovalRejectedNotificationAsync(
                            creator.Email,
                            tender.Title,
                            detailsUrl
                        )
                        .Wait();
                }
            }

            return new SuccessResult("Onay işlemi başarıyla tamamlandı.");
        }

    }
}