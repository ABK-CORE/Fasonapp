using Core.Utilities.Result.Abstract;
using Entities.Dtos;

namespace Business.Abstract
{
    public interface ISupplierManagementService
    {
        IDataResult<SupplierDto> CreateSupplier(SupplierCreateDto dto);
        IDataResult<SupplierDto> UpdateSupplier(SupplierUpdateDto dto);
        IResult DeleteSupplier(Guid userGuid);
        IDataResult<SupplierDto> GetSupplier(Guid userGuid);
        IDataResult<List<SupplierDto>> GetSuppliers();
    }
}
