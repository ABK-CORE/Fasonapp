// src/Business/Concrete/SupplierManagementManager.cs
using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Enigma;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace Business.Concrete
{
    public class SupplierManagementManager : ISupplierManagementService
    {
        private readonly IUserDal _userDal;
        private readonly IUserRoleDal _userRoleDal;
        private readonly IRoleDal _roleDal;
        private readonly Processor _processor;
        private readonly IMapper _mapper;
        private readonly ISupplierFirmInfoDal _firmInfoDal;
        private readonly ISupplierContactInfoDal _contactInfoDal;

        public SupplierManagementManager(
            IUserDal userDal,
            IUserRoleDal userRoleDal,
            IRoleDal roleDal,
            ISupplierFirmInfoDal firmInfoDal,
            ISupplierContactInfoDal contactInfoDal,
            Processor processor,
            IMapper mapper)
        {
            _userDal = userDal;
            _userRoleDal = userRoleDal;
            _roleDal = roleDal;
            _firmInfoDal = firmInfoDal;
            _contactInfoDal = contactInfoDal;
            _processor = processor;
            _mapper = mapper;
        }

        public IDataResult<SupplierDto> CreateSupplier(SupplierCreateDto dto)
        {
            // 1) E-posta/username kontrol
            if (_userDal.GetList(u => u.Email == dto.Email).Any())
                return new ErrorDataResult<SupplierDto>(null, SystemMessages.ResourceAlreadyExists);

            // 2) Şifre şifrele
            using var aes = Aes.Create();
            var hashed = _processor.EncryptorSymmetric(dto.Password, aes);

            // 3) User oluştur
            var user = _mapper.Map<User>(dto);
            user.Password = hashed;
            user.IsActive = true;
            _userDal.Add(user);

            // 4) SupplierFirmInfo ekle
            var firmInfo = new SupplierFirmInfo
            {
                UserGuid = user.Guid,
                CompanyName = dto.CompanyName,
                TaxOffice = dto.TaxOffice,
                TaxNumber = dto.TaxNumber,
                TradeRegisterNumber = dto.TradeRegisterNumber,
                CompanyType = dto.CompanyType,
                MerisNo = dto.MerisNo
            };
            _firmInfoDal.Add(firmInfo);

            // 5) SupplierContactInfo ekle
            var contactInfo = new SupplierContactInfo
            {
                UserGuid = user.Guid,
                ContactName = dto.ContactName,
                ContactPosition = dto.ContactPosition,
                PhoneNumber = dto.ContactPhoneNumber,
                Email = dto.ContactEmail,
                Website = dto.ContactWebsite,
                Address = dto.ContactAddress
            };
            _contactInfoDal.Add(contactInfo);

            // 6) Rol ata
            var role = _roleDal.Get(r => r.RoleName == "Supplier");
            _userRoleDal.Add(new UserRole
            {
                UserGuid = user.Guid,
                RoleId = role.RoleId,
                AssignedDate = DateTime.UtcNow,
                UserId = user.Id
            });

            // 7) DTO’ya map et ve döndür
            var resultDto = _mapper.Map<SupplierDto>(user);
            return new SuccessDataResult<SupplierDto>(resultDto, SystemMessages.OperationSuccessful);
        }

        public IDataResult<SupplierDto> UpdateSupplier(SupplierUpdateDto dto)
        {
            var user = _userDal.Get(u => u.Guid == dto.Guid);
            if (user == null)
                return new ErrorDataResult<SupplierDto>(null, SystemMessages.RecordNotFound);

            // 1) User alanlarını güncelle
            var pass = user.Password;
            _mapper.Map(dto, user);
            user.Password = pass;
            _userDal.Update(user);

            // 2) Firma Bilgileri güncelle veya ekle
            var firm = _firmInfoDal.Get(f => f.UserGuid == dto.Guid);
            if (firm == null)
            {
                firm = new SupplierFirmInfo { UserGuid = dto.Guid };
                _firmInfoDal.Add(firm);
            }
            _mapper.Map(dto, firm);
            _firmInfoDal.Update(firm);

            // 3) İletişim Bilgileri
            var contact = _contactInfoDal.Get(c => c.UserGuid == dto.Guid);
            if (contact == null)
            {
                contact = new SupplierContactInfo { UserGuid = dto.Guid };
                _contactInfoDal.Add(contact);
            }
            _mapper.Map(dto, contact);
            _contactInfoDal.Update(contact);

            var resultDto = _mapper.Map<SupplierDto>(user);
            return new SuccessDataResult<SupplierDto>(resultDto, SystemMessages.OperationSuccessful);
        }

        public IResult DeleteSupplier(Guid userGuid)
        {
            var entity = _userDal.Get(u => u.Guid == userGuid);
            if (entity == null)
                return new Result(false, SystemMessages.RecordNotFound);

            entity.IsActive = !entity.IsActive;

            _userDal.Update(entity);
            return new SuccessResult(SystemMessages.OperationSuccessful);
        }

        public IDataResult<List<SupplierDto>> GetSuppliers()
        {
            var supplierRole = _roleDal.Get(r => r.RoleName == "Supplier");
            if (supplierRole == null)
                return new ErrorDataResult<List<SupplierDto>>(null, SystemMessages.RecordNotFound);

            var supplierUsers = (from ur in _userRoleDal.GetList(ur => ur.RoleId == supplierRole.RoleId)
                                 join u in _userDal.GetList() on ur.UserGuid equals u.Guid
                                 join fi in _firmInfoDal.GetList() on u.Guid equals fi.UserGuid into firmJoin
                                 from firmInfo in firmJoin.DefaultIfEmpty()
                                 join ci in _contactInfoDal.GetList() on u.Guid equals ci.UserGuid into contactJoin
                                 from contactInfo in contactJoin.DefaultIfEmpty()
                                 select new SupplierDto
                                 {
                                     Guid = u.Guid,
                                     Email = u.Email,
                                     FirstName = u.FirstName,
                                     LastName = u.LastName,
                                     IsActive = u.IsActive,
                                     PhoneNumber = u.PhoneNumber,

                                     // Firma bilgileri
                                     CompanyName = firmInfo.CompanyName,
                                     TaxOffice = firmInfo.TaxOffice,
                                     TaxNumber = firmInfo.TaxNumber,
                                     TradeRegisterNumber = firmInfo.TradeRegisterNumber,
                                     CompanyType = firmInfo.CompanyType,
                                     MerisNo = firmInfo.MerisNo,

                                     // İletişim bilgileri
                                     ContactName = contactInfo.ContactName,
                                     ContactEmail = contactInfo.Email,
                                     ContactPhoneNumber = contactInfo.PhoneNumber,
                                     ContactWebsite = contactInfo.Website,
                                     ContactPosition = contactInfo.ContactPosition,
                                     ContactAddress = contactInfo.Address
                                 }).ToList();

            return new SuccessDataResult<List<SupplierDto>>(supplierUsers);
        }

        public IDataResult<SupplierDto> GetSupplier(Guid userGuid)
        {
            var user = _userDal.Get(
                u => u.Guid == userGuid,
                u => u.FirmInfo,
                u => u.ContactInfo
            );

            if (user == null)
                return new ErrorDataResult<SupplierDto>(null, SystemMessages.RecordNotFound);

            var dto = _mapper.Map<SupplierDto>(user);
            return new SuccessDataResult<SupplierDto>(dto);
        }
    }
}
