using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IPartService
    {
        IDataResult<List<PartDto>> GetParts();
        IDataResult<int> AddPart(PartCreateDto dto);
        IDataResult<bool> DeletePart(int partId);
        IDataResult<bool> UpdatePart(PartUpdateDto dto);
    }
}
