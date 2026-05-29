using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPurchaseCategoryService
    {
        IDataResult<List<PurchaseCategoryDto>> GetAll();
        IDataResult<PurchaseCategoryDto> GetById(int categoryId);
        IDataResult<int> Create(PurchaseCategoryCreateDto dto);
        IDataResult<bool> Update(PurchaseCategoryUpdateDto dto);
        IResult Delete(int categoryId);
    }
}
