using Core.Utilities.Result.Abstract;
using Entities.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Business.Abstract
{
    public interface IApprovalRuleService
    {
        IDataResult<ApprovalRulesSetupDto> GetSetup();
        IDataResult<ApprovalRulesSetupDto> SaveAllSetup(ApprovalRulesSetupDto setup);
        IResult DeleteRule(int ruleId);
        IDataResult<List<ApproverDto>> GetApproversByAmount(decimal amount);
    }
}
