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
[Route("api/era/players")]
[RequestSizeLimit(16384)]
[OpenApiTagOrder(401)]
public sealed class EraPlayersController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraPlayersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("me")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraPlayer")]
    [ProducesResponseType(typeof(GetEraPlayerResponse), 200)]
    public async Task<IActionResult> GetPlayerAsync(
        [FromQuery] GetEraPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraPlayerRequest, GetEraPlayerResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("me/initialize")]
    [SkipPermissionCheck]
    [DescriptionLocalization("InitializeEraPlayer")]
    [ProducesResponseType(typeof(InitializeEraPlayerResponse), 200)]
    public async Task<IActionResult> InitializePlayerAsync(
        [FromBody] InitializeEraPlayerCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<InitializeEraPlayerCommand, InitializeEraPlayerResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }
}
