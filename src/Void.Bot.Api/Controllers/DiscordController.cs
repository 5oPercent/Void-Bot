using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Commands.Discord;
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

    [HttpGet]
    [Route("voice-channel/config")]
    [ProducesResponseType(typeof(GetDiscordVoiceChannelConfigResponse), 200)]
    public async Task<IActionResult> GetVoiceChannelConfigAsync([FromQuery] GetDiscordVoiceChannelConfigRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordVoiceChannelConfigRequest, GetDiscordVoiceChannelConfigResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("voice-channel/config/list")]
    [ProducesResponseType(typeof(GetDiscordVoiceChannelConfigListResponse), 200)]
    public async Task<IActionResult> GetVoiceChannelConfigListAsync([FromQuery] GetDiscordVoiceChannelConfigListRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordVoiceChannelConfigListRequest, GetDiscordVoiceChannelConfigListResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/add")]
    [ProducesResponseType(typeof(AddDiscordVoiceChannelConfigResponse), 200)]
    public async Task<IActionResult> AddVoiceChannelConfigAsync([FromBody] AddDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<AddDiscordVoiceChannelConfigCommand, AddDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/update")]
    [ProducesResponseType(typeof(UpdateDiscordVoiceChannelConfigResponse), 200)]
    public async Task<IActionResult> UpdateVoiceChannelConfigAsync([FromBody] UpdateDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<UpdateDiscordVoiceChannelConfigCommand, UpdateDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/delete")]
    [ProducesResponseType(typeof(DeleteDiscordVoiceChannelConfigResponse), 200)]
    public async Task<IActionResult> DeleteVoiceChannelConfigAsync([FromBody] DeleteDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<DeleteDiscordVoiceChannelConfigCommand, DeleteDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("voice-channel/temporary/list")]
    [ProducesResponseType(typeof(GetDiscordTemporaryVoiceChannelListResponse), 200)]
    public async Task<IActionResult> GetTemporaryVoiceChannelListAsync([FromQuery] GetDiscordTemporaryVoiceChannelListRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordTemporaryVoiceChannelListRequest, GetDiscordTemporaryVoiceChannelListResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }
}
