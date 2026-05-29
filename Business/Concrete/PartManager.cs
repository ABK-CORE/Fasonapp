using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class PartManager : IPartService
    {
        private readonly IPartDal _partDal;
        private readonly IMapper _mapper;
        public PartManager(IPartDal partDal, IMapper mapper)
        {
            _partDal = partDal;
            _mapper = mapper;
        }

        public IDataResult<List<PartDto>> GetParts()
        {
            var data = _partDal.GetList(x=> x.IsActive);
            var dtos = _mapper.Map<List<PartDto>>(data);
            return new SuccessDataResult<List<PartDto>>(dtos);
        }

        public IDataResult<int> AddPart(PartCreateDto dto)
        {
            var entity = _mapper.Map<Part>(dto);
            entity.IsActive = true;
            _partDal.Add(entity);
            return new SuccessDataResult<int>(entity.PartId, "Parça başarıyla eklendi.");
        }

        public IDataResult<bool> DeletePart(int partId)
        {
            var part = _partDal.Get(p => p.PartId == partId);

            if (part == null)
                return new ErrorDataResult<bool>("Silinecek parça bulunamadı.");

            part.IsActive = false;

            _partDal.Update(part);
            return new SuccessDataResult<bool>(true, "Parça başarıyla silindi.");
        }

        public IDataResult<bool> UpdatePart(PartUpdateDto dto)
        {
            var part = _partDal.Get(p => p.PartId == dto.PartId);
            if (part == null)
                return new ErrorDataResult<bool>("Güncellenecek parça bulunamadı.");

            part.PartCode = dto.PartCode;
            part.PartName = dto.PartName;
            part.Description = dto.Description;
            if (!string.IsNullOrEmpty(dto.PartPhoto))
                part.PartPhoto = dto.PartPhoto;

            _partDal.Update(part);
            return new SuccessDataResult<bool>(true, "Parça başarıyla güncellendi.");
        }

    }
}
