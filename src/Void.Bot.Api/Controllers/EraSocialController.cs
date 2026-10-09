using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize(AuthenticationSchemes = "Bearer")]
[EraClientSession]
[SocialAccount]
[ApiController]
[Security]
[Route("api/era/social")]
[RequestSizeLimit(8192)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[EnableRateLimiting("social")]
public class EraSocialController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraSocialController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("friends/statuses")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraFriendStatuses")]
    [ProducesResponseType(typeof(GetEraFriendStatusesResponse), 200)]
    public async Task<IActionResult> GetEraFriendStatusesAsync([FromQuery] GetEraFriendStatusesRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraFriendStatusesRequest, GetEraFriendStatusesResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("friends/{friendAccountId:guid}/team")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraFriendTarget")]
    [ProducesResponseType(typeof(GetEraFriendTargetResponse), 200)]
    public async Task<IActionResult> GetEraFriendTargetAsync(Guid friendAccountId, [FromQuery] GetEraFriendTargetRequest message,
        CancellationToken cancellationToken)
    {
        message.FriendAccountId = friendAccountId;
        var response = await _mediator.RequestAsync<GetEraFriendTargetRequest, GetEraFriendTargetResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("friends/join")]
    [SkipPermissionCheck]
    [DescriptionLocalization("JoinEraFriendTeam")]
    [ProducesResponseType(typeof(JoinEraFriendTeamResponse), 200)]
    public async Task<IActionResult> JoinEraFriendTeamAsync([FromBody] JoinEraFriendTeamCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<JoinEraFriendTeamCommand, JoinEraFriendTeamResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPut("teams/{teamId:guid}/settings")]
    [SkipPermissionCheck]
    [DescriptionLocalization("UpdateEraTeamSettings")]
    [ProducesResponseType(typeof(UpdateEraTeamSettingsResponse), 200)]
    public async Task<IActionResult> UpdateEraTeamSettingsAsync(Guid teamId, [FromBody] UpdateEraTeamSettingsCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator.SendAsync<UpdateEraTeamSettingsCommand, UpdateEraTeamSettingsResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("invitations")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SendEraTeamInvitation")]
    [ProducesResponseType(typeof(SendEraTeamInvitationResponse), 200)]
    public async Task<IActionResult> SendEraTeamInvitationAsync([FromBody] SendEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SendEraTeamInvitationCommand, SendEraTeamInvitationResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("invitations")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraTeamInvitations")]
    [ProducesResponseType(typeof(GetEraTeamInvitationsResponse), 200)]
    public async Task<IActionResult> GetEraTeamInvitationsAsync([FromQuery] GetEraTeamInvitationsRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraTeamInvitationsRequest, GetEraTeamInvitationsResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("invitations/{invitationId:guid}")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraTeamInvitation")]
    [ProducesResponseType(typeof(GetEraTeamInvitationResponse), 200)]
    public async Task<IActionResult> GetEraTeamInvitationAsync(Guid invitationId, [FromQuery] GetEraTeamInvitationRequest message,
        CancellationToken cancellationToken)
    {
        message.InvitationId = invitationId;
        var response = await _mediator.RequestAsync<GetEraTeamInvitationRequest, GetEraTeamInvitationResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("invitations/{invitationId:guid}/withdraw")]
    [SkipPermissionCheck]
    [DescriptionLocalization("WithdrawEraTeamInvitation")]
    [ProducesResponseType(typeof(WithdrawEraTeamInvitationResponse), 200)]
    public async Task<IActionResult> WithdrawEraTeamInvitationAsync(Guid invitationId, [FromBody] WithdrawEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        message.InvitationId = invitationId;
        var response = await _mediator.SendAsync<WithdrawEraTeamInvitationCommand, WithdrawEraTeamInvitationResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("invitations/{invitationId:guid}/reject")]
    [SkipPermissionCheck]
    [DescriptionLocalization("RejectEraTeamInvitation")]
    [ProducesResponseType(typeof(RejectEraTeamInvitationResponse), 200)]
    public async Task<IActionResult> RejectEraTeamInvitationAsync(Guid invitationId, [FromBody] RejectEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        message.InvitationId = invitationId;
        var response = await _mediator.SendAsync<RejectEraTeamInvitationCommand, RejectEraTeamInvitationResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("invitations/{invitationId:guid}/accept")]
    [SkipPermissionCheck]
    [DescriptionLocalization("AcceptEraTeamInvitation")]
    [ProducesResponseType(typeof(AcceptEraTeamInvitationResponse), 200)]
    public async Task<IActionResult> AcceptEraTeamInvitationAsync(Guid invitationId, [FromBody] AcceptEraTeamInvitationCommand message,
        CancellationToken cancellationToken)
    {
        message.InvitationId = invitationId;
        var response = await _mediator.SendAsync<AcceptEraTeamInvitationCommand, AcceptEraTeamInvitationResponse>(message,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

}
