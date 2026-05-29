// Business/Concrete/ApprovalRuleManager.cs
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;

namespace Business.Concrete
{
    public class ApprovalRuleManager : IApprovalRuleService
    {
        private readonly IApprovalRuleDal _ruleDal;
        private readonly IApprovalRuleApproverDal _approverDal;
        private readonly IPreApprovalApproverDal _preApproverDal;
        private readonly IAuditLogDal _logDal;
        private readonly IMapper _mapper;

        public ApprovalRuleManager(
            IApprovalRuleDal ruleDal,
            IApprovalRuleApproverDal approverDal,
            IPreApprovalApproverDal preApproverDal,
            IAuditLogDal logDal,
            IMapper mapper)
        {
            _ruleDal = ruleDal;
            _approverDal = approverDal;
            _preApproverDal = preApproverDal;
            _logDal = logDal;
            _mapper = mapper;
        }

        public IDataResult<ApprovalRulesSetupDto> GetSetup()
        {
            // 1) Global ön-onaycıları sırayla çek
            var preList = _preApproverDal
                .GetList()
                .OrderBy(p => p.OrderIndex)
                .ToList();

            // 2) Fiyat-temelli kuralları ve onaycılarını çek
            var rules = _ruleDal
                .GetList(includeProperties: r => r.Approvers)
                .ToList();

            // 3) DTO’ya elle dönüştür
            var setupDto = new ApprovalRulesSetupDto
            {
                PreApprovers = _mapper.Map<List<ApproverDto>>(preList),
                Rules = rules.Select(r => new ApprovalRuleCreateDto
                {
                    MinAmount = r.MinAmount,
                    MaxAmount = r.MaxAmount,
                    Approvers = r.Approvers
                                 .OrderBy(a => a.OrderIndex)
                                 .Select(a => new ApproverDto
                                 {
                                     UserGuid = a.UserGuid,
                                     OrderIndex = a.OrderIndex
                                 })
                                 .ToList()
                }).ToList()
            };

            // Audit
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = Guid.Empty,        // system/user context if you have it
                Category = "ApprovalRules",
                Message = "Onay kuralları görüntülendi.",
                RequestId = null,
                Type = "Info"
            });

            return new SuccessDataResult<ApprovalRulesSetupDto>(setupDto);
        }

        public IDataResult<ApprovalRulesSetupDto> SaveAllSetup(ApprovalRulesSetupDto setup)
        {
            // 1) Ön-onaycıları sil + ekle
            foreach (var p in _preApproverDal.GetList())
                _preApproverDal.Delete(p);
            foreach (var p in setup.PreApprovers)
                _preApproverDal.Add(new PreApprovalApprover
                {
                    UserGuid = p.UserGuid,
                    OrderIndex = p.OrderIndex
                });

            // 2) Fiyat-temelli kuralları ve onaycıları sil
            foreach (var a in _approverDal.GetList()) _approverDal.Delete(a);
            foreach (var r in _ruleDal.GetList()) _ruleDal.Delete(r);

            // 3) Yeni kuralları ekle
            foreach (var rDto in setup.Rules)
            {
                var rule = new ApprovalRule
                {
                    MinAmount = rDto.MinAmount,
                    MaxAmount = rDto.MaxAmount
                };
                _ruleDal.Add(rule);

                foreach (var a in rDto.Approvers)
                {
                    _approverDal.Add(new ApprovalRuleApprover
                    {
                        RuleId = rule.RuleId,
                        UserGuid = a.UserGuid,
                        OrderIndex = a.OrderIndex
                    });
                }
            }

            // Audit
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = Guid.Empty,
                Category = "ApprovalRules",
                Message = "Onay kuralları güncellendi.",
                RequestId = null,
                Type = "Info"
            });

            // 4) Kaydettikten sonra güncel veriyi dön
            var result = GetSetup();
            return new SuccessDataResult<ApprovalRulesSetupDto>(result.Data);
        }

        public IResult DeleteRule(int ruleId)
        {
            var rule = _ruleDal.Get(r => r.RuleId == ruleId);
            if (rule == null)
                return new Result(false, "Kural bulunamadı.");

            _ruleDal.Delete(rule);

            // Audit
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = Guid.Empty,
                Category = "ApprovalRules",
                Message = $"Onay kuralı (ID={ruleId}) silindi.",
                RequestId = null,
                Type = "Warning"
            });

            return new SuccessResult("Kural silindi.");
        }

        public IDataResult<List<ApproverDto>> GetApproversByAmount(decimal amount)
        {
            // 1) Fiyat aralığına uyan kuralı bul
            var rule = _ruleDal
                .GetList(r => r.MinAmount <= amount && amount <= r.MaxAmount,
                         includeProperties: r => r.Approvers)
                .FirstOrDefault();

            if (rule == null)
            {
                return new ErrorDataResult<List<ApproverDto>>(null, "Fiyat aralığında kural bulunamadı.");
            }

            // 2) Kuralın onaycıları
            var approvers = rule.Approvers
                .OrderBy(a => a.OrderIndex)
                .Select(a => new ApproverDto
                {
                    UserGuid = a.UserGuid,
                    OrderIndex = a.OrderIndex
                })
                .ToList();

            if (!approvers.Any())
            {
                return new ErrorDataResult<List<ApproverDto>>(null, "Fiyat aralığında onaycı bulunamadı.");
            }

            // 3) Audit log
            _logDal.Add(new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                UserGuid = Guid.Empty,      // System context
                Category = "ApprovalRules",
                Message = $"Tutar {amount} için onaycılar getirildi (Kural ID={rule.RuleId}).",
                RequestId = null,
                Type = "Info"
            });

            return new SuccessDataResult<List<ApproverDto>>(approvers);
        }
    }
}
