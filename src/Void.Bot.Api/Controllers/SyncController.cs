using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Commands.Sync;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private IMediator _mediator;

    public SyncController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [AllowAnonymous]
    [HttpPost]
    [Route("destiny2/manifest")]
    public async Task<IActionResult> SynchronizeDestiny2ManifestAsync([FromBody]SynchronizeDestiny2ManifestCommand command)
    {
        var response = await _mediator.SendAsync<SynchronizeDestiny2ManifestCommand, SynchronizeDestiny2ManifestResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
}