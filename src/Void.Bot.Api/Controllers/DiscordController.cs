using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiscordController : ControllerBase
{
    private IMediator _mediator;

    public DiscordController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Route("bot/start")]
    public async Task StartBotAsync([FromQuery]DiscordStartBotRequest request)
    {
        await _mediator.RequestAsync<DiscordStartBotRequest, DiscordStartBotResponse>(request).ConfigureAwait(false);
    }
    
    [HttpGet]
    [Route("bot/stop")]
    public async Task StopBotAsync([FromQuery]DiscordStopBotRequest request)
    {
        await _mediator.RequestAsync<DiscordStopBotRequest, DiscordStopBotResponse>(request).ConfigureAwait(false);
    }
}