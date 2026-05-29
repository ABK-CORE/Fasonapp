using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IContractService
    {
        IDataResult<List<ContractDto>> GetAll();
        IResult Add(ContractCreateDto dto, Guid currentUserGuid, List<IFormFile>? contractFiles);
        IResult Delete(Guid contractGuid, Guid currentUserGuid);
        IDataResult<List<ContractDto>> GetBySupplier(Guid supplierGuid);
        IDataResult<List<ContractWithPartsDto>> GetActiveContractsWithParts(Guid supplierGuid);
    }
}
