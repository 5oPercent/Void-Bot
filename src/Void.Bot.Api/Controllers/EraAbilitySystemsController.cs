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
[Route("api/era/characters/{characterId:guid}/ability-systems")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(403)]
public sealed class EraAbilitySystemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraAbilitySystemsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("loadouts")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraSkillProfile")]
    [ProducesResponseType(typeof(GetEraSkillProfileResponse), 200)]
    public async Task<IActionResult> GetProfileAsync(
        [FromRoute] Guid characterId, [FromQuery] GetEraSkillProfileRequest request,
        CancellationToken cancellationToken)
    {
        request.CharacterId = characterId;
        var response = await _mediator
            .RequestAsync<GetEraSkillProfileRequest, GetEraSkillProfileResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost("loadouts")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SaveEraSkillProfile")]
    [ProducesResponseType(typeof(SaveEraSkillProfileResponse), 200)]
    public async Task<IActionResult> SaveProfileAsync(
        [FromRoute] Guid characterId, [FromBody] SaveEraSkillProfileCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<SaveEraSkillProfileCommand, SaveEraSkillProfileResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }
}
