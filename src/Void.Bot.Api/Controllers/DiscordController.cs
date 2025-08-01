using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
public class DiscordController : ControllerBase
{
    private readonly IMediator _mediator;

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
    
    [HttpGet]
    [Route("bot/apply/global/slashCommand")]
    public async Task ApplyGlobalSlashCommandAsync([FromQuery]DiscordApplyGlobalSlashCommandRequest request)
    {
        await _mediator.RequestAsync<DiscordApplyGlobalSlashCommandRequest, DiscordApplyGlobalSlashCommandResponse>(request).ConfigureAwait(false);
    }
    
    [HttpGet]
    [Route("bot/apply/guild/slashCommand")]
    public async Task ApplyGuildSlashCommandAsync([FromQuery]DiscordApplyGuildSlashCommandRequest request)
    {
        await _mediator.RequestAsync<DiscordApplyGuildSlashCommandRequest, DiscordApplyGuildSlashCommandResponse>(request).ConfigureAwait(false);
    }
}