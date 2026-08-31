using Void.Bot.Message.Attributes;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Commands.EventAction;
using Void.Bot.Message.Request.EventAction;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/event-action")]
[Security]
[OpenApiTagOrder(220)]
public class EventActionController(IMediator mediator) : ControllerBase
{
    [HttpGet("definitions")]
    [ProducesResponseType(typeof(GetEventActionDefinitionResponse), 200)]
    [DescriptionLocalization("GetDefinitions")]
    public async Task<IActionResult> GetDefinitionsAsync()
    {
        var response = await mediator.RequestAsync<GetEventActionDefinitionRequest, GetEventActionDefinitionResponse>(
            new GetEventActionDefinitionRequest()).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("rules")]
    [ProducesResponseType(typeof(GetEventActionRuleListResponse), 200)]
    [DescriptionLocalization("GetRules")]
    public async Task<IActionResult> GetRulesAsync([FromQuery] GetEventActionRuleListRequest request)
    {
        var response = await mediator.RequestAsync<GetEventActionRuleListRequest, GetEventActionRuleListResponse>(
            request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("executions")]
    [ProducesResponseType(typeof(GetEventActionExecutionPageResponse), 200)]
    [DescriptionLocalization("GetExecutions")]
    public async Task<IActionResult> GetExecutionsAsync([FromQuery] GetEventActionExecutionPageRequest request)
    {
        var response = await mediator.RequestAsync<
            GetEventActionExecutionPageRequest,
            GetEventActionExecutionPageResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("rules/add")]
    [ProducesResponseType(typeof(AddEventActionRuleResponse), 200)]
    [DescriptionLocalization("AddRule")]
    public async Task<IActionResult> AddRuleAsync([FromBody] AddEventActionRuleCommand command)
    {
        var response = await mediator.SendAsync<AddEventActionRuleCommand, AddEventActionRuleResponse>(
            command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("rules/update")]
    [ProducesResponseType(typeof(UpdateEventActionRuleResponse), 200)]
    [DescriptionLocalization("UpdateRule")]
    public async Task<IActionResult> UpdateRuleAsync([FromBody] UpdateEventActionRuleCommand command)
    {
        var response = await mediator.SendAsync<UpdateEventActionRuleCommand, UpdateEventActionRuleResponse>(
            command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("rules/delete")]
    [ProducesResponseType(typeof(DeleteEventActionRuleResponse), 200)]
    [DescriptionLocalization("DeleteRules")]
    public async Task<IActionResult> DeleteRulesAsync([FromBody] DeleteEventActionRuleCommand command)
    {
        var response = await mediator.SendAsync<DeleteEventActionRuleCommand, DeleteEventActionRuleResponse>(
            command).ConfigureAwait(false);
        return Ok(response);
    }
}
