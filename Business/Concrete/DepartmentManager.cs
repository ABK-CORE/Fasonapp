using AutoMapper;
using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;

namespace Business.Concrete
{
    public class DepartmentManager : IDepartmentService
    {
        private readonly IDepartmentDal _departmentDal;
        private readonly IDepartmentUserDal _departmentUserDal;
        private readonly IUserDal _userDal;
        private readonly IMapper _mapper;

        public DepartmentManager(
            IDepartmentDal departmentDal,
            IDepartmentUserDal departmentUserDal,
            IUserDal userDal,
            IMapper mapper)
        {
            _departmentDal = departmentDal;
            _departmentUserDal = departmentUserDal;
            _userDal = userDal;
            _mapper = mapper;
        }

        public IDataResult<DepartmentDto> CreateDepartment(DepartmentCreateDto dto)
        {
            var department = new Department
            {
                Guid = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };
            _departmentDal.Add(department);

            var resultDto = _mapper.Map<DepartmentDto>(department);
            return new SuccessDataResult<DepartmentDto>(resultDto, "Departman oluşturuldu.");
        }

        public IDataResult<List<DepartmentDto>> GetAllDepartments()
        {
            var list = _departmentDal.GetList(d => d.IsActive);
            var dtos = _mapper.Map<List<DepartmentDto>>(list);
            return new SuccessDataResult<List<DepartmentDto>>(dtos);
        }

        public IDataResult<DepartmentDto> GetDepartmentByGuid(Guid departmentGuid)
        {
            var dept = _departmentDal.Get(d => d.Guid == departmentGuid && d.IsActive);
            if (dept == null)
                return new ErrorDataResult<DepartmentDto>(null, "Departman bulunamadı.");

            var dto = _mapper.Map<DepartmentDto>(dept);
            return new SuccessDataResult<DepartmentDto>(dto);
        }

        public IResult DeleteDepartment(Guid departmentGuid)
        {
            var dept = _departmentDal.Get(d => d.Guid == departmentGuid);
            if (dept == null)
                return new Result(false, "Departman bulunamadı.");

            dept.IsActive = false;
            _departmentDal.Update(dept);
            return new SuccessResult("Departman pasif hale getirildi.");
        }

        public IResult AddUserToDepartment(DepartmentUserCreateDto dto)
        {
            var dept = _departmentDal.Get(d => d.Guid == dto.DepartmentGuid && d.IsActive);
            if (dept == null)
                return new Result(false, "Departman bulunamadı.");

            var user = _userDal.Get(u => u.Guid == dto.UserGuid);
            if (user == null)
                return new Result(false, "Kullanıcı bulunamadı.");

            bool exists = _departmentUserDal.GetList(x =>
                x.DepartmentId == dept.DepartmentId && x.UserGuid == dto.UserGuid).Any();
            if (exists)
                return new Result(false, "Kullanıcı zaten bu departmanda.");

            var du = new DepartmentUser
            {
                DepartmentId = dept.DepartmentId,
                UserGuid = dto.UserGuid,
                IsManager = dto.IsManager,
                ManagerLevel = dto.ManagerLevel,
                AssignedDate = DateTime.UtcNow
            };
            _departmentUserDal.Add(du);
            return new SuccessResult("Kullanıcı departmana eklendi.");
        }

        public IResult RemoveUserFromDepartment(Guid departmentGuid, Guid userGuid)
        {
            var dept = _departmentDal.Get(d => d.Guid == departmentGuid);
            if (dept == null)
                return new Result(false, "Departman bulunamadı.");

            var du = _departmentUserDal.Get(x =>
                x.DepartmentId == dept.DepartmentId && x.UserGuid == userGuid);
            if (du == null)
                return new Result(false, "Kullanıcı bu departmanda değil.");

            _departmentUserDal.Delete(du);
            return new SuccessResult("Kullanıcı departmandan çıkarıldı.");
        }

        public IDataResult<List<DepartmentUserDto>> GetDepartmentUsers(Guid departmentGuid)
        {
            var dept = _departmentDal.Get(d => d.Guid == departmentGuid && d.IsActive);
            if (dept == null)
                return new ErrorDataResult<List<DepartmentUserDto>>(null, "Departman bulunamadı.");

            var users = _departmentUserDal.GetList(x => x.DepartmentId == dept.DepartmentId, x => x.User);
            var dtos = users.Select(x => new DepartmentUserDto
            {
                UserGuid = x.UserGuid,
                UserName = $"{x.User.FirstName} {x.User.LastName}",
                IsManager = x.IsManager,
                ManagerLevel = x.ManagerLevel,
                AssignedDate = x.AssignedDate
            }).ToList();

            return new SuccessDataResult<List<DepartmentUserDto>>(dtos);
        }
    }
}