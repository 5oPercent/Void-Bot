using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Commands.Sync;
using Void.Bot.Message.Request.Sync;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
public class SyncController : ControllerBase
{
    private readonly IMediator _mediator;

    public SyncController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 获取 Destiny 2 Manifest 可同步的语言和清单标识。
    /// </summary>
    [HttpGet]
    [Route("destiny2/manifest/options")]
    public async Task<IActionResult> GetDestiny2ManifestSyncOptionsAsync()
    {
        var response = await _mediator
            .RequestAsync<GetDestiny2ManifestSyncOptionsRequest, GetDestiny2ManifestSyncOptionsResponse>(
                new GetDestiny2ManifestSyncOptionsRequest())
            .ConfigureAwait(false);

        return Ok(response);
    }
    
    /// <summary>
    /// 同步 Destiny 2 Manifest 原始清单，不创建武器派生缓存。
    /// </summary>
    [HttpPost]
    [Route("destiny2/manifest")]
    public async Task<IActionResult> SynchronizeDestiny2ManifestAsync([FromBody]SynchronizeDestiny2ManifestCommand command)
    {
        var response = await _mediator.SendAsync<SynchronizeDestiny2ManifestCommand, SynchronizeDestiny2ManifestResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
}
