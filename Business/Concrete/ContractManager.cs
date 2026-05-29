using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Business.Concrete
{
    public class ContractManager : IContractService
    {
        private readonly IContractsDal _contractDal;
        private readonly IContractFilesDal _fileDal;
        private readonly IContractPartsDal _partsDal;
        private readonly IPartDal _partDal;

        public ContractManager(IContractsDal contractDal, IContractFilesDal fileDal, IContractPartsDal partsDal, IPartDal partDal)
        {
            _contractDal = contractDal;
            _fileDal = fileDal;
            _partsDal = partsDal;
            _partDal = partDal;
        }

        public IDataResult<List<ContractDto>> GetAll()
        {
            var entities = _contractDal.GetList(
                c => c.IsActive,
                c => c.Files,
                c => c.Parts
            );

            var list = entities.Select(c => new ContractDto
            {
                ContractGuid = c.ContractGuid,
                SupplierGuid = c.SupplierGuid,
                Title = c.Title,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                FilePath = c.Files.Select(f => f.FilePath).ToList(),
                ContractType = c.ContractType,
                RecurrencePattern = c.RecurrencePattern,
                Parts = c.Parts.Select(p => new PartPriceDto { PartId = p.PartId, UnitPrice = p.UnitPrice }).ToList()
            }).ToList();

            return new SuccessDataResult<List<ContractDto>>(list);
        }


        public IResult Add(
             ContractCreateDto dto,
             Guid currentUserGuid,
             List<IFormFile>? contractFiles)
        {
            if (dto.EndDate < dto.StartDate)
                return new Result(false, "Bitiş tarihi başlangıçtan önce olamaz.");

            // 1️⃣ Contract ekle
            var contract = new Contract
            {
                SupplierGuid = dto.SupplierGuid,
                Title = dto.Title,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreatedDate = DateTime.UtcNow,
                Status = 1,
                CreatedBy = currentUserGuid,
                ContractType = dto.ContractType,
                RecurrencePattern = dto.RecurrencePattern
            };
            _contractDal.Add(contract);

            // Dosyaların kaydedileceği klasör
            var folder = Path.Combine(Directory.GetCurrentDirectory(), "MainFile");
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            // 2️⃣ Birden çok dosya varsa kaydet ve veritabanına ekle
            if (contractFiles != null)
            {
                foreach (var file in contractFiles)
                {
                    if (file.Length <= 0)
                        continue;

                    var ext = Path.GetExtension(file.FileName);
                    var fileName = $"{Guid.NewGuid()}{ext}";
                    var fullPath = Path.Combine(folder, fileName);
                    using var stream = new FileStream(fullPath, FileMode.Create);
                    file.CopyTo(stream);

                    var cf = new ContractFile
                    {
                        ContractId = contract.ContractId,
                        FileName = fileName,
                        FilePath = $"/{fileName}",
                        UploadedDate = DateTime.UtcNow
                    };
                    _fileDal.Add(cf);
                }
            }

            // 3️⃣ Parça bazlı fiyatlama varsa ekle
            foreach (var p in dto.Parts)
            {
                var cp = new ContractPart
                {
                    ContractId = contract.ContractId,
                    PartId = p.PartId,
                    UnitPrice = p.UnitPrice
                };
                _partsDal.Add(cp);
            }

            return new SuccessResult("Sözleşme başarıyla eklendi.");
        }

        public IResult Delete(Guid contractGuid, Guid currentUserGuid)
        {
            var contract = _contractDal.Get(c => c.ContractGuid == contractGuid && c.IsActive);
            if (contract == null)
                return new Result(false, "Sözleşme bulunamadı veya zaten silinmiş.");

            contract.IsActive = false;
            contract.ModifiedDate = DateTime.UtcNow;
            contract.ModifiedBy = currentUserGuid;
            _contractDal.Update(contract);

            return new SuccessResult("Sözleşme başarıyla pasifleştirildi.");
        }

        public IDataResult<List<ContractDto>> GetBySupplier(Guid supplierGuid)
        {
            // Sadece aktif ve SupplierGuid eşleşenleri getir
            var entities = _contractDal.GetList(
                c => c.IsActive && c.SupplierGuid == supplierGuid,
                c => c.Files,
                c => c.Parts
            );

            var list = entities.Select(c => new ContractDto
            {
                ContractGuid = c.ContractGuid,
                SupplierGuid = c.SupplierGuid,
                Title = c.Title,
                Description = c.Description,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                FilePath = c.Files.Select(f => f.FilePath).ToList(),
                ContractType = c.ContractType,
                RecurrencePattern = c.RecurrencePattern,
                Parts = c.Parts
                                      .Select(p => new PartPriceDto
                                      {
                                          PartId = p.PartId,
                                          UnitPrice = p.UnitPrice
                                      }).ToList()
            }).ToList();

            return new SuccessDataResult<List<ContractDto>>(list);
        }

        public IDataResult<List<ContractWithPartsDto>> GetActiveContractsWithParts(Guid supplierGuid)
        {
            var now = DateTime.UtcNow;
            // Aktif ve tarih aralığında olan sözleşmeleri, parçalarıyla birlikte çek
            var contracts = _contractDal.GetList(
                c => c.IsActive
                     && c.SupplierGuid == supplierGuid
                     && c.StartDate <= now
                     && c.EndDate >= now,
                c => c.Parts
            );

            var result = contracts.Select(c => new ContractWithPartsDto
            {
                ContractId = c.ContractId,
                ContractGuid = c.ContractGuid,
                Title = c.Title,
                Parts = c.Parts
                         .Select(p => new PartPriceDto
                         {
                             PartId = p.PartId,
                             UnitPrice = p.UnitPrice
                         })
                         .ToList()
            }).ToList();

            return new SuccessDataResult<List<ContractWithPartsDto>>(result);
        }
    }
}