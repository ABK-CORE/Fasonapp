using AutoMapper;
using Core.Entities;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enum;
using System;

namespace Business.DependencyRepository.AutoMapper
{
    public class Maps : Profile
    {
        public Maps()
        {
            // 1) DTO → User (temel kullanıcı bilgileri)
            CreateMap<SupplierCreateDto, User>()
                .ForMember(d => d.Guid,
                           opt => opt.MapFrom(_ => Guid.NewGuid()))
                .ForMember(d => d.CreatedDate,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            CreateMap<SupplierUpdateDto, User>()
                .ForMember(d => d.DateOfBirth,
                           opt => opt.MapFrom(src =>
                               src.DateOfBirth.HasValue
                                   ? DateOnly.FromDateTime(src.DateOfBirth.Value)
                                   : (DateOnly?)null))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // 2) DTO → Firma Bilgileri Entity
            CreateMap<SupplierCreateDto, SupplierFirmInfo>()
                .ForMember(d => d.UserGuid, opt => opt.Ignore())
                .ForMember(d => d.SupplierFirmInfoId, opt => opt.Ignore());

            CreateMap<SupplierUpdateDto, SupplierFirmInfo>()
                .ForMember(d => d.UserGuid, opt => opt.Ignore())
                .ForMember(d => d.SupplierFirmInfoId, opt => opt.Ignore());

            // 3) DTO → İletişim Bilgileri Entity
            CreateMap<SupplierCreateDto, SupplierContactInfo>()
                .ForMember(d => d.UserGuid, opt => opt.Ignore())
                .ForMember(d => d.SupplierContactInfoId, opt => opt.Ignore());

            CreateMap<SupplierUpdateDto, SupplierContactInfo>()
                .ForMember(d => d.UserGuid, opt => opt.Ignore())
                .ForMember(d => d.SupplierContactInfoId, opt => opt.Ignore());

            // 4) Entity → DTO (tek bir SupplierDto’ya topluyoruz)
            CreateMap<User, SupplierDto>()
                .ForMember(d => d.DateOfBirth,
                           opt => opt.MapFrom(src =>
                               src.DateOfBirth.HasValue
                                   ? src.DateOfBirth.Value.ToDateTime(new TimeOnly(0, 0))
                                   : (DateTime?)null))
                // Firma Bilgileri
                .ForMember(d => d.CompanyName,
                           opt => opt.MapFrom(src => src.FirmInfo!.CompanyName))
                .ForMember(d => d.TaxOffice,
                           opt => opt.MapFrom(src => src.FirmInfo!.TaxOffice))
                .ForMember(d => d.TaxNumber,
                           opt => opt.MapFrom(src => src.FirmInfo!.TaxNumber))
                .ForMember(d => d.TradeRegisterNumber,
                           opt => opt.MapFrom(src => src.FirmInfo!.TradeRegisterNumber))
                .ForMember(d => d.CompanyType,
                           opt => opt.MapFrom(src => src.FirmInfo!.CompanyType))
                .ForMember(d => d.MerisNo,
                           opt => opt.MapFrom(src => src.FirmInfo!.MerisNo))
                // İletişim Bilgileri
                .ForMember(d => d.ContactName,
                           opt => opt.MapFrom(src => src.ContactInfo!.ContactName))
                .ForMember(d => d.ContactPosition,
                           opt => opt.MapFrom(src => src.ContactInfo!.ContactPosition))
                .ForMember(d => d.ContactPhoneNumber,
                           opt => opt.MapFrom(src => src.ContactInfo!.PhoneNumber))
                .ForMember(d => d.ContactEmail,
                           opt => opt.MapFrom(src => src.ContactInfo!.Email))
                .ForMember(d => d.ContactWebsite,
                           opt => opt.MapFrom(src => src.ContactInfo!.Website))
                .ForMember(d => d.ContactAddress,
                           opt => opt.MapFrom(src => src.ContactInfo!.Address));


            // ——— Part mappings ———
            CreateMap<Part, PartDto>();

            CreateMap<ApprovalRule, ApprovalRuleDto>()
                // Fiyat-temelli onaycıları entity.Approvers koleksiyonundan al
                .ForMember(dest => dest.Approvers,
                           opt => opt.MapFrom(src => src.Approvers))
                // PreApprovers'i manuel ekleyeceğimiz için AutoMapper’a pas geç
                .ForMember(dest => dest.PreApprovers,
                           opt => opt.Ignore());

            CreateMap<ApprovalRuleApprover, ApprovalRuleApproverDto>();

            CreateMap<ApprovalRuleDto, ApprovalRule>()
                .ForMember(d => d.Approvers, o => o.Ignore());

            CreateMap<ApprovalRuleApproverDto, ApprovalRuleApprover>()
                .ForMember(d => d.Rule, o => o.Ignore());


            // DTO → Entity (kural)
            // Yeni kural kaydı DTO’su → ApprovalRule
            CreateMap<ApprovalRuleCreateDto, ApprovalRule>()
                .ForMember(dest => dest.Approvers,
                           opt => opt.Ignore());

            // DTO → Entity (her bir onaycı satırı)
            CreateMap<(Guid UserGuid, int OrderIndex), ApprovalRuleApprover>()
                .ForMember(d => d.UserGuid, o => o.MapFrom(src => src.UserGuid))
                .ForMember(d => d.OrderIndex, o => o.MapFrom(src => src.OrderIndex))
                .ForMember(d => d.RuleId, o => o.Ignore())
                .ForMember(d => d.RuleApproverId, o => o.Ignore());

            CreateMap<Tender, TenderDto>()
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category))
                .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items))
                .ForMember(d => d.Offers, opt => opt.MapFrom(s => s.Offers))
                .ForMember(d => d.ApprovalRecords, opt => opt.MapFrom(s => s.ApprovalRecords))
                .ForMember(d => d.Category, opt => opt.MapFrom(s => s.Category));

            CreateMap<TenderCreateDto, Tender>();
            CreateMap<TenderItem, TenderItemDto>();
            CreateMap<TenderItemCreateDto, TenderItem>();

            CreateMap<ApprovalRecord, ApprovalRecordDto>()
                .ForMember(dest => dest.UserGuid,
                           opt => opt.MapFrom(src => src.UserGuid))
                .ForMember(dest => dest.UserName,
                           opt => opt.Ignore())
                .ForMember(dest => dest.OrderIndex,
                           opt => opt.MapFrom(src => src.OrderIndex))
                .ForMember(dest => dest.IsApproved,
                           opt => opt.MapFrom(src => src.IsApproved))
                .ForMember(dest => dest.ActionDate,
                           opt => opt.MapFrom(src =>
                                src.ActionDate.HasValue
                                  ? src.ActionDate.Value
                                  : (DateTime?)null))
                .ForMember(dest => dest.RecordType,
                           opt => opt.MapFrom(src => src.RecordType));

            // Ön-onaycı tablosu → ApproverDto
            CreateMap<PreApprovalApprover, ApproverDto>()
                .ForMember(d => d.UserGuid, o => o.MapFrom(src => src.UserGuid))
                .ForMember(d => d.OrderIndex, o => o.MapFrom(src => src.OrderIndex));

            // Fiyat-temelli onaycı satırı → ApproverDto
            CreateMap<ApprovalRuleApprover, ApproverDto>()
                .ForMember(d => d.UserGuid, o => o.MapFrom(src => src.UserGuid))
                .ForMember(d => d.OrderIndex, o => o.MapFrom(src => src.OrderIndex));

            CreateMap<AuditLog, AuditLogDto>()
                .ForMember(d => d.LogId, o => o.MapFrom(s => s.LogId))
                .ForMember(d => d.Timestamp, o => o.MapFrom(s => s.Timestamp))
                .ForMember(d => d.UserGuid, o => o.MapFrom(s => s.UserGuid))
                .ForMember(d => d.Category, o => o.MapFrom(s => s.Category))
                .ForMember(d => d.Message, o => o.MapFrom(s => s.Message))
                .ForMember(d => d.RequestId, o => o.MapFrom(s => s.RequestId))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type));

            CreateMap<Role, RoleDto>()
                .ForMember(dest => dest.Id,
                           opt => opt.MapFrom(src => src.RoleId))
                .ForMember(dest => dest.Name,
                           opt => opt.MapFrom(src => src.RoleName))
                .ForMember(dest => dest.Description,
                           opt => opt.MapFrom(src => src.Description));

            CreateMap<PartCreateDto, Part>();

            CreateMap<TenderOffer, TenderOfferDto>()
                .ForMember(d => d.OfferId, opt => opt.MapFrom(s => s.OfferId))
                .ForMember(d => d.OfferGuid, opt => opt.MapFrom(s => s.OfferGuid))
                .ForMember(d => d.TenderGuid, opt => opt.MapFrom(s => s.TenderGuid))
                .ForMember(d => d.SupplierId, opt => opt.MapFrom(s => s.SupplierId))
                .ForMember(d => d.PartId, opt => opt.MapFrom(s => s.PartId))
                .ForMember(d => d.Price, opt => opt.MapFrom(s => s.UnitPrice))
                .ForMember(d => d.CreatedDate, opt => opt.MapFrom(s => s.CreatedDate))
                .ForMember(d => d.IsAccepted, opt => opt.MapFrom(s => s.IsAccepted))
                .ForMember(d => d.PartName, opt => opt.MapFrom(s => s.Part.PartName))
                .ForMember(d => d.PartCode, opt => opt.MapFrom(s => s.Part.PartCode));

            // Entity → DTO
            CreateMap<PurchaseCategory, PurchaseCategoryDto>()
                .ForMember(d => d.LimitPeriod,
                           opt => opt.MapFrom(src => (byte)src.LimitPeriod));

            // CreateDto → Entity
            CreateMap<PurchaseCategoryCreateDto, PurchaseCategory>()
                .ForMember(d => d.CreatedDate,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.LimitPeriod,
                           opt => opt.MapFrom(src => (LimitPeriodType)src.LimitPeriod))
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // UpdateDto → Entity
            CreateMap<PurchaseCategoryUpdateDto, PurchaseCategory>()
                .ForMember(d => d.UpdatedDate,
                           opt => opt.MapFrom(_ => DateTime.UtcNow))
                .ForMember(d => d.LimitPeriod,
                           opt => opt.MapFrom(src => (LimitPeriodType)src.LimitPeriod))
                .ForMember(d => d.CreatedDate, opt => opt.Ignore())  // CreatedDate'i koru
                .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

            // PurchaseCategory → TenderCategoryDto ———
            CreateMap<PurchaseCategory, TenderCategoryDto>()
                .ForMember(d => d.CategoryId,
                           opt => opt.MapFrom(src => src.CategoryId))
                .ForMember(d => d.Name,
                           opt => opt.MapFrom(src => src.Name));

            CreateMap<Department, DepartmentDto>();
            CreateMap<DepartmentCreateDto, Department>();

            CreateMap<PurchaseRequest, PurchaseRequestDto>()
                .ForMember(dest => dest.CategoryName,
                           opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<PurchaseRequestItem, PurchaseRequestItemDto>()
                .ForMember(dest => dest.Unit,
                           opt => opt.MapFrom(src => src.Unit));

            // CreateDto → Entity
            CreateMap<PurchaseRequestCreateDto, PurchaseRequest>()
                .ForMember(dest => dest.CategoryId,
                           opt => opt.MapFrom(src => src.CategoryId));

            CreateMap<PurchaseRequestItemCreateDto, PurchaseRequestItem>()
                .ForMember(dest => dest.Unit,
                           opt => opt.MapFrom(src => src.Unit));

        }
    }
}
