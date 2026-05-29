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

namespace Business.Concrete
{
    public class PurchaseCategoryManager : IPurchaseCategoryService
    {
        private readonly IPurchaseCategoryDal _categoryDal;
        private readonly IMapper _mapper;

        public PurchaseCategoryManager(IPurchaseCategoryDal categoryDal, IMapper mapper)
        {
            _categoryDal = categoryDal;
            _mapper = mapper;
        }

        public IDataResult<List<PurchaseCategoryDto>> GetAll()
        {
            var list = _categoryDal.GetList(c => c.IsActive).ToList();
            var dto = _mapper.Map<List<PurchaseCategoryDto>>(list);
            return new SuccessDataResult<List<PurchaseCategoryDto>>(dto);
        }

        public IDataResult<PurchaseCategoryDto> GetById(int categoryId)
        {
            var entity = _categoryDal.Get(c => c.CategoryId == categoryId && c.IsActive);
            if (entity == null)
                return new ErrorDataResult<PurchaseCategoryDto>("Kategori bulunamadı.");
            var dto = _mapper.Map<PurchaseCategoryDto>(entity);
            return new SuccessDataResult<PurchaseCategoryDto>(dto);
        }

        public IDataResult<int> Create(PurchaseCategoryCreateDto dto)
        {
            var entity = _mapper.Map<PurchaseCategory>(dto);
            entity.IsActive = true;
            entity.CreatedDate = DateTime.Now;
            _categoryDal.Add(entity);
            return new SuccessDataResult<int>(entity.CategoryId, "Kategori eklendi.");
        }

        public IDataResult<bool> Update(PurchaseCategoryUpdateDto dto)
        {
            var entity = _categoryDal.Get(c => c.CategoryId == dto.CategoryId);
            if (entity == null)
                return new ErrorDataResult<bool>("Kategori bulunamadı.");

            entity.Name = dto.Name;
            entity.LimitAmount = dto.LimitAmount;
            entity.LimitPeriod = (Entities.Enum.LimitPeriodType)dto.LimitPeriod;
            entity.UpdatedDate = DateTime.Now;
            _categoryDal.Update(entity);

            return new SuccessDataResult<bool>(true, "Kategori güncellendi.");
        }

        public IResult Delete(int categoryId)
        {
            var entity = _categoryDal.Get(c => c.CategoryId == categoryId && c.IsActive);
            if (entity == null)
                return new Result(false, "Kategori bulunamadı.");
            entity.IsActive = false;
            _categoryDal.Update(entity);
            return new SuccessResult("Kategori silindi.");
        }
    }
}