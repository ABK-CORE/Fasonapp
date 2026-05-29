using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IDepartmentApprovalProcessService
    {
        IDataResult<List<ApprovalProcessStepDto>> GetSteps(Guid departmentGuid);
        IResult SaveSteps(Guid departmentGuid, List<ApprovalProcessStepDto> steps);
    }
}
