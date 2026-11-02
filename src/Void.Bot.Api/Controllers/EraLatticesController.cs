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
public sealed class EraLatticesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraLatticesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("loadouts")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraSkillProfile")]
    [ProducesResponseType(typeof(GetEraResonanceProfileResponse), 200)]
    public async Task<IActionResult> GetProfileAsync(
        [FromRoute] Guid characterId, [FromQuery] GetEraResonanceProfileRequest request,
        CancellationToken cancellationToken)
    {
        request.CharacterId = characterId;
        var response = await _mediator
            .RequestAsync<GetEraResonanceProfileRequest, GetEraResonanceProfileResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost("loadouts")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SaveEraSkillProfile")]
    [ProducesResponseType(typeof(SaveEraResonanceProfileResponse), 200)]
    public async Task<IActionResult> SaveProfileAsync(
        [FromRoute] Guid characterId, [FromBody] SaveEraResonanceProfileCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<SaveEraResonanceProfileCommand, SaveEraResonanceProfileResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }
}
