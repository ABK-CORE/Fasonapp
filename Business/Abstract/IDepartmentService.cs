using Core.Utilities.Result.Abstract;
using Entities.Dtos;

namespace Business.Abstract
{
    public interface IDepartmentService
    {
        IDataResult<DepartmentDto> CreateDepartment(DepartmentCreateDto dto);
        IDataResult<List<DepartmentDto>> GetAllDepartments();
        IDataResult<DepartmentDto> GetDepartmentByGuid(Guid departmentGuid);
        IResult DeleteDepartment(Guid departmentGuid);

        IResult AddUserToDepartment(DepartmentUserCreateDto dto);
        IResult RemoveUserFromDepartment(Guid departmentGuid, Guid userGuid);
        IDataResult<List<DepartmentUserDto>> GetDepartmentUsers(Guid departmentGuid);
    }
}
