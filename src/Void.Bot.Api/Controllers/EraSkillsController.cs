using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize]
[EraClientSession]
[ApiController]
[Security]
[Route("api/era")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(201)]
public class EraSkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraSkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("skills/config")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraSkillConfiguration")]
    [ProducesResponseType(typeof(GetEraSkillConfigurationResponse), 200)]
    public async Task<IActionResult> GetConfigurationAsync(
        [FromQuery] GetEraSkillConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraSkillConfigurationRequest, GetEraSkillConfigurationResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("me/skill-loadout")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraSkillProfile")]
    [ProducesResponseType(typeof(GetEraSkillProfileResponse), 200)]
    public async Task<IActionResult> GetProfileAsync(
        [FromQuery] GetEraSkillProfileRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraSkillProfileRequest, GetEraSkillProfileResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("me/learned-skills")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraLearnedSkills")]
    [ProducesResponseType(typeof(GetEraLearnedSkillsResponse), 200)]
    public async Task<IActionResult> GetLearnedSkillsAsync(
        [FromQuery] GetEraLearnedSkillsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraLearnedSkillsRequest, GetEraLearnedSkillsResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPut("admin/skills/config")]
    [DescriptionLocalization("PublishEraSkillConfiguration")]
    [ProducesResponseType(typeof(PublishEraSkillConfigurationResponse), 200)]
    public async Task<IActionResult> PublishAsync(
        [FromBody] PublishEraSkillConfigurationCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<PublishEraSkillConfigurationCommand, PublishEraSkillConfigurationResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPut("me/skill-loadout")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SaveEraSkillProfile")]
    [ProducesResponseType(typeof(SaveEraSkillProfileResponse), 200)]
    public async Task<IActionResult> SaveProfileAsync(
        [FromBody] SaveEraSkillProfileCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<SaveEraSkillProfileCommand, SaveEraSkillProfileResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost("admin/players/{playerId:guid}/learned-skills")]
    [DescriptionLocalization("GrantEraLearnedSkill")]
    [ProducesResponseType(typeof(GrantEraLearnedSkillResponse), 200)]
    public async Task<IActionResult> GrantAsync(
        [FromRoute] Guid playerId,
        [FromBody] GrantEraLearnedSkillCommand command,
        CancellationToken cancellationToken)
    {
        command.PlayerId = playerId;
        var response = await _mediator
            .SendAsync<GrantEraLearnedSkillCommand, GrantEraLearnedSkillResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

}
