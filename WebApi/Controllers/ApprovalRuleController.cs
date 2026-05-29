using Business.Abstract;
using Entities.Dtos;
using Infrastructure.Middleware;
using Microsoft.AspNetCore.Mvc;

[Route("api/approval-rule")]
[ApiController]
public class ApprovalRuleController : ControllerBase
{
    private readonly IApprovalRuleService _service;
    public ApprovalRuleController(IApprovalRuleService service)
        => _service = service;

    // GET api/approval-rule
    [HttpGet]
    public IActionResult GetAll()
        => Ok(_service.GetSetup());

    // GET api/approval-rule/setup
    [HttpGet("setup")]
    public IActionResult GetSetupEndpoint()
    {
        var result = _service.GetSetup();
        return Ok(result);
    }

    // POST api/approval-rule
    [HttpPost]
    [RequireDbRole("ApproverManagement")]
    public IActionResult SaveAllSetup([FromBody] ApprovalRulesSetupDto setupDto)
    {
        var result = _service.SaveAllSetup(setupDto);
        return Ok(result);
    }

    // DELETE api/approval-rule/{ruleId}
    [HttpDelete("{ruleId:int}")]
    [RequireDbRole("ApproverManagement")]
    public IActionResult Delete(int ruleId)
        => Ok(_service.DeleteRule(ruleId));
}
