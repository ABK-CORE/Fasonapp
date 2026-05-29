using Business.Abstract;
using Core.Utilities.Result.Abstract;
using Core.Utilities.Result.Concrete;
using DataAccess.Abstract;
using Entities.Concrete.EntityFramework.Entities;
using Entities.Dtos;
using Entities.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class DepartmentApprovalProcessManager : IDepartmentApprovalProcessService
    {
        private readonly IDepartmentApprovalProcessDal _procDal;
        private readonly IApprovalProcessStepDal _stepDal;
        private readonly IUserDal _userDal;
        private readonly IDepartmentDal _deptDal;

        public DepartmentApprovalProcessManager(
            IDepartmentApprovalProcessDal procDal,
            IApprovalProcessStepDal stepDal,
            IUserDal userDal,
            IDepartmentDal deptDal)
        {
            _procDal = procDal;
            _stepDal = stepDal;
            _userDal = userDal;
            _deptDal = deptDal;
        }

        public IDataResult<List<ApprovalProcessStepDto>> GetSteps(Guid departmentGuid)
        {
            var dept = _deptDal.Get(d => d.Guid == departmentGuid);
            if (dept == null)
                return new ErrorDataResult<List<ApprovalProcessStepDto>>(null, "Departman bulunamadı.");

            // Süreç yoksa null döner
            var proc = _procDal.Get(p => p.DepartmentId == dept.DepartmentId, p => p.Steps);
            if (proc == null)
            {
                // Henüz süreç tanımlanmadı, boş liste dön
                return new SuccessDataResult<List<ApprovalProcessStepDto>>(
                    new List<ApprovalProcessStepDto>(),
                    "Henüz onay süreci tanımlanmadı."
                );
            }

            var dtos = proc.Steps
                .OrderBy(s => s.OrderIndex)
                .Select(s => new ApprovalProcessStepDto
                {
                    OrderIndex = s.OrderIndex,
                    StepType = s.StepType,
                    UserGuid = s.UserGuid,
                    ManagerLevel = s.ManagerLevel
                })
                .ToList();

            return new SuccessDataResult<List<ApprovalProcessStepDto>>(dtos);
        }

        public IResult SaveSteps(Guid departmentGuid, List<ApprovalProcessStepDto> steps)
        {
            var dept = _deptDal.Get(d => d.Guid == departmentGuid);
            if (dept == null) return new Result(false, "Departman bulunamadı.");

            // Mevcut süreci sil + adımları sil
            var existing = _procDal.Get(p => p.DepartmentId == dept.DepartmentId);
            if (existing != null)
            {
                foreach (var st in _stepDal.GetList(s => s.ProcessId == existing.ProcessId))
                    _stepDal.Delete(st);
                _procDal.Delete(existing);
            }

            // Yeni süreci ekle
            var proc = new DepartmentApprovalProcess
            {
                DepartmentId = dept.DepartmentId,
                Name = $"{dept.Name} Onay Süreci"
            };
            _procDal.Add(proc);

            // Adımları ekle
            foreach (var dto in steps.OrderBy(s => s.OrderIndex))
            {
                _stepDal.Add(new ApprovalProcessStep
                {
                    ProcessId = proc.ProcessId,
                    OrderIndex = dto.OrderIndex,
                    StepType = dto.StepType,
                    UserGuid = dto.StepType == StepType.User ? dto.UserGuid : null,
                    ManagerLevel = dto.StepType == StepType.ManagerLevel ? dto.ManagerLevel : null
                });
            }

            return new SuccessResult("Onay süreci kaydedildi.");
        }
    }
}
