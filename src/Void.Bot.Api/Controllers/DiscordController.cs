using Void.Bot.Message.Attributes;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Commands.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(300)]
public class DiscordController : ControllerBase
{
    private readonly IMediator _mediator;

    public DiscordController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Route("bot/start")]
    [ProducesResponseType(typeof(DiscordStartBotResponse), 200)]
    [DescriptionLocalization("StartBot")]
    public async Task<IActionResult> StartBotAsync([FromQuery]DiscordStartBotRequest request)
    {
        var response = await _mediator.RequestAsync<DiscordStartBotRequest, DiscordStartBotResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }
    
    [HttpGet]
    [Route("bot/stop")]
    [ProducesResponseType(typeof(DiscordStopBotResponse), 200)]
    [DescriptionLocalization("StopBot")]
    public async Task<IActionResult> StopBotAsync([FromQuery]DiscordStopBotRequest request)
    {
        var response = await _mediator.RequestAsync<DiscordStopBotRequest, DiscordStopBotResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }
    
    [HttpGet]
    [Route("bot/apply/global/slashCommand")]
    [ProducesResponseType(typeof(DiscordApplyGlobalSlashCommandResponse), 200)]
    [DescriptionLocalization("ApplyGlobalSlashCommand")]
    public async Task<IActionResult> ApplyGlobalSlashCommandAsync([FromQuery]DiscordApplyGlobalSlashCommandRequest request)
    {
        var response = await _mediator.RequestAsync<DiscordApplyGlobalSlashCommandRequest, DiscordApplyGlobalSlashCommandResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }
    
    [HttpGet]
    [Route("bot/apply/guild/slashCommand")]
    [ProducesResponseType(typeof(DiscordApplyGuildSlashCommandResponse), 200)]
    [DescriptionLocalization("ApplyGuildSlashCommand")]
    public async Task<IActionResult> ApplyGuildSlashCommandAsync([FromQuery]DiscordApplyGuildSlashCommandRequest request)
    {
        var response = await _mediator.RequestAsync<DiscordApplyGuildSlashCommandRequest, DiscordApplyGuildSlashCommandResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("managed-guilds")]
    [ProducesResponseType(typeof(GetDiscordManagedGuildListResponse), 200)]
    [DescriptionLocalization("GetManagedGuildList")]
    public async Task<IActionResult> GetManagedGuildListAsync(
        [FromQuery] GetDiscordManagedGuildListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetDiscordManagedGuildListRequest, GetDiscordManagedGuildListResponse>(
                request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("guilds/channels")]
    [ProducesResponseType(typeof(GetDiscordGuildChannelListResponse), 200)]
    [DescriptionLocalization("GetGuildChannelList")]
    public async Task<IActionResult> GetGuildChannelListAsync(
        [FromQuery] GetDiscordGuildChannelListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetDiscordGuildChannelListRequest, GetDiscordGuildChannelListResponse>(
                request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("guilds/roles")]
    [ProducesResponseType(typeof(GetDiscordGuildRoleListResponse), 200)]
    [DescriptionLocalization("GetGuildRoleList")]
    public async Task<IActionResult> GetGuildRoleListAsync(
        [FromQuery] GetDiscordGuildRoleListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetDiscordGuildRoleListRequest, GetDiscordGuildRoleListResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("role-panel/config")]
    [ProducesResponseType(typeof(GetDiscordRolePanelConfigResponse), 200)]
    [DescriptionLocalization("GetRolePanelConfig")]
    public async Task<IActionResult> GetRolePanelConfigAsync(
        [FromQuery] GetDiscordRolePanelConfigRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetDiscordRolePanelConfigRequest, GetDiscordRolePanelConfigResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("role-panel/config/save")]
    [ProducesResponseType(typeof(SaveDiscordRolePanelResponse), 200)]
    [DescriptionLocalization("SaveRolePanelConfig")]
    public async Task<IActionResult> SaveRolePanelConfigAsync(
        [FromBody] SaveDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<SaveDiscordRolePanelCommand, SaveDiscordRolePanelResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("role-panel/config/delete")]
    [ProducesResponseType(typeof(DeleteDiscordRolePanelResponse), 200)]
    [DescriptionLocalization("DeleteRolePanelConfig")]
    public async Task<IActionResult> DeleteRolePanelConfigAsync(
        [FromBody] DeleteDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<DeleteDiscordRolePanelCommand, DeleteDiscordRolePanelResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("role-panel/publish")]
    [ProducesResponseType(typeof(PublishDiscordRolePanelResponse), 200)]
    [DescriptionLocalization("PublishRolePanel")]
    public async Task<IActionResult> PublishRolePanelAsync(
        [FromBody] PublishDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<PublishDiscordRolePanelCommand, PublishDiscordRolePanelResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("role-panel/refresh")]
    [ProducesResponseType(typeof(RefreshDiscordRolePanelResponse), 200)]
    [DescriptionLocalization("RefreshRolePanel")]
    public async Task<IActionResult> RefreshRolePanelAsync(
        [FromBody] RefreshDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<RefreshDiscordRolePanelCommand, RefreshDiscordRolePanelResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("role-panel/unpublish")]
    [ProducesResponseType(typeof(UnpublishDiscordRolePanelResponse), 200)]
    [DescriptionLocalization("UnpublishRolePanel")]
    public async Task<IActionResult> UnpublishRolePanelAsync(
        [FromBody] UnpublishDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<UnpublishDiscordRolePanelCommand, UnpublishDiscordRolePanelResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("voice-channel/config")]
    [ProducesResponseType(typeof(GetDiscordVoiceChannelConfigResponse), 200)]
    [DescriptionLocalization("GetVoiceChannelConfig")]
    public async Task<IActionResult> GetVoiceChannelConfigAsync([FromQuery] GetDiscordVoiceChannelConfigRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordVoiceChannelConfigRequest, GetDiscordVoiceChannelConfigResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("voice-channel/config/list")]
    [ProducesResponseType(typeof(GetDiscordVoiceChannelConfigListResponse), 200)]
    [DescriptionLocalization("GetVoiceChannelConfigList")]
    public async Task<IActionResult> GetVoiceChannelConfigListAsync([FromQuery] GetDiscordVoiceChannelConfigListRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordVoiceChannelConfigListRequest, GetDiscordVoiceChannelConfigListResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/add")]
    [ProducesResponseType(typeof(AddDiscordVoiceChannelConfigResponse), 200)]
    [DescriptionLocalization("AddVoiceChannelConfig")]
    public async Task<IActionResult> AddVoiceChannelConfigAsync([FromBody] AddDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<AddDiscordVoiceChannelConfigCommand, AddDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/update")]
    [ProducesResponseType(typeof(UpdateDiscordVoiceChannelConfigResponse), 200)]
    [DescriptionLocalization("UpdateVoiceChannelConfig")]
    public async Task<IActionResult> UpdateVoiceChannelConfigAsync([FromBody] UpdateDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<UpdateDiscordVoiceChannelConfigCommand, UpdateDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("voice-channel/config/delete")]
    [ProducesResponseType(typeof(DeleteDiscordVoiceChannelConfigResponse), 200)]
    [DescriptionLocalization("DeleteVoiceChannelConfig")]
    public async Task<IActionResult> DeleteVoiceChannelConfigAsync([FromBody] DeleteDiscordVoiceChannelConfigCommand command)
    {
        var response = await _mediator.SendAsync<DeleteDiscordVoiceChannelConfigCommand, DeleteDiscordVoiceChannelConfigResponse>(command).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("voice-channel/temporary/list")]
    [ProducesResponseType(typeof(GetDiscordTemporaryVoiceChannelListResponse), 200)]
    [DescriptionLocalization("GetTemporaryVoiceChannelList")]
    public async Task<IActionResult> GetTemporaryVoiceChannelListAsync([FromQuery] GetDiscordTemporaryVoiceChannelListRequest request)
    {
        var response = await _mediator.RequestAsync<GetDiscordTemporaryVoiceChannelListRequest, GetDiscordTemporaryVoiceChannelListResponse>(request).ConfigureAwait(false);
        return Ok(response);
    }
}
