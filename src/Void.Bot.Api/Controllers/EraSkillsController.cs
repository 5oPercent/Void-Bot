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
[Route("api/era/characters/{characterId:guid}/skills")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(404)]
public sealed class EraSkillsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraSkillsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("learned")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraLearnedSkills")]
    [ProducesResponseType(typeof(GetEraLearnedSkillsResponse), 200)]
    public async Task<IActionResult> GetLearnedSkillsAsync(
        [FromRoute] Guid characterId, [FromQuery] GetEraLearnedSkillsRequest request,
        CancellationToken cancellationToken)
    {
        request.CharacterId = characterId;
        var response = await _mediator
            .RequestAsync<GetEraLearnedSkillsRequest, GetEraLearnedSkillsResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }
}
