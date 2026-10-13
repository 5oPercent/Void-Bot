using Void.Bot.Api.OpenApi;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Security]
[EraClientSession]
[Route("api/era/session")]
[OpenApiTagOrder(401)]
public class EraClientSessionController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraClientSessionController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("heartbeat")]
    [SkipPermissionCheck]
    [DescriptionLocalization("HeartbeatEraClientSession")]
    [ProducesResponseType(typeof(HeartbeatEraClientSessionResponse), 200)]
    public async Task<IActionResult> HeartbeatAsync([FromBody] HeartbeatEraClientSessionCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = Guid.Parse(Request.Headers["X-ERA-Client-Id"].ToString());
        var response = await _mediator.SendAsync<HeartbeatEraClientSessionCommand, HeartbeatEraClientSessionResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("close")]
    [SkipPermissionCheck]
    [DescriptionLocalization("CloseEraClientSession")]
    [ProducesResponseType(typeof(CloseEraClientSessionResponse), 200)]
    public async Task<IActionResult> CloseAsync([FromBody] CloseEraClientSessionCommand command, CancellationToken cancellationToken)
    {
        command.ClientId = Guid.Parse(Request.Headers["X-ERA-Client-Id"].ToString());
        var response = await _mediator.SendAsync<CloseEraClientSessionCommand, CloseEraClientSessionResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
