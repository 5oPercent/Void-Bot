using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Constants.ERA;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize]
[EraClientSession]
[ApiController]
[Security]
[Route("api/era/teams")]
[RequestSizeLimit(4096)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[EnableRateLimiting(EraNetworkingConstants.TeamRateLimitPolicy)]
[OpenApiTagOrder(402)]
public class EraTeamController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraTeamController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [SkipPermissionCheck]
    [DescriptionLocalization("CreateEraTeam")]
    [ProducesResponseType(typeof(CreateEraTeamResponse), 200)]
    public async Task<IActionResult> CreateAsync(
        [FromBody] CreateEraTeamCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<CreateEraTeamCommand, CreateEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("join")]
    [SkipPermissionCheck]
    [DescriptionLocalization("JoinEraTeam")]
    [ProducesResponseType(typeof(JoinEraTeamResponse), 200)]
    public async Task<IActionResult> JoinAsync(
        [FromBody] JoinEraTeamCommand message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<JoinEraTeamCommand, JoinEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/heartbeat")]
    [SkipPermissionCheck]
    [DescriptionLocalization("HeartbeatEraTeam")]
    [ProducesResponseType(typeof(HeartbeatEraTeamResponse), 200)]
    public async Task<IActionResult> HeartbeatAsync(
        Guid teamId,
        [FromBody] HeartbeatEraTeamCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<HeartbeatEraTeamCommand, HeartbeatEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/leave")]
    [SkipPermissionCheck]
    [DescriptionLocalization("LeaveEraTeam")]
    [ProducesResponseType(typeof(LeaveEraTeamResponse), 200)]
    public async Task<IActionResult> LeaveAsync(
        Guid teamId,
        [FromBody] LeaveEraTeamCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<LeaveEraTeamCommand, LeaveEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/punch")]
    [SkipPermissionCheck]
    [DescriptionLocalization("PrepareEraPunch")]
    [ProducesResponseType(typeof(PrepareEraPunchResponse), 200)]
    public async Task<IActionResult> PreparePunchAsync(
        Guid teamId,
        [FromBody] PrepareEraPunchCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<PrepareEraPunchCommand, PrepareEraPunchResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/host-readiness")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SetEraHostReadiness")]
    [ProducesResponseType(typeof(SetEraHostReadinessResponse), 200)]
    public async Task<IActionResult> SetHostReadinessAsync(
        Guid teamId,
        [FromBody] SetEraHostReadinessCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<SetEraHostReadinessCommand, SetEraHostReadinessResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/migration-complete")]
    [SkipPermissionCheck]
    [DescriptionLocalization("CompleteEraHostMigration")]
    [ProducesResponseType(typeof(CompleteEraHostMigrationResponse), 200)]
    public async Task<IActionResult> CompleteHostMigrationAsync(
        Guid teamId,
        [FromBody] CompleteEraHostMigrationCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<CompleteEraHostMigrationCommand, CompleteEraHostMigrationResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/switch")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SwitchEraTeam")]
    [ProducesResponseType(typeof(SwitchEraTeamResponse), 200)]
    public async Task<IActionResult> SwitchAsync(
        Guid teamId,
        [FromBody] SwitchEraTeamCommand message,
        CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator
            .SendAsync<SwitchEraTeamCommand, SwitchEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/transfer-host")]
    [SkipPermissionCheck]
    [DescriptionLocalization("TransferEraTeamMember")]
    [ProducesResponseType(typeof(TransferEraTeamMemberResponse), 200)]
    public async Task<IActionResult> TransferHostAsync(Guid teamId, [FromBody] TransferEraTeamMemberCommand message, CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator.SendAsync<TransferEraTeamMemberCommand, TransferEraTeamMemberResponse>(message, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{teamId:guid}/remove-member")]
    [SkipPermissionCheck]
    [DescriptionLocalization("RemoveEraTeamMember")]
    [ProducesResponseType(typeof(RemoveEraTeamMemberResponse), 200)]
    public async Task<IActionResult> RemoveMemberAsync(Guid teamId, [FromBody] RemoveEraTeamMemberCommand message, CancellationToken cancellationToken)
    {
        message.TeamId = teamId;
        var response = await _mediator.SendAsync<RemoveEraTeamMemberCommand, RemoveEraTeamMemberResponse>(message, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("lookup/{teamCode}")]
    [SkipPermissionCheck]
    [DescriptionLocalization("LookupEraTeam")]
    [ProducesResponseType(typeof(LookupEraTeamResponse), 200)]
    public async Task<IActionResult> LookupAsync(string teamCode, CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<LookupEraTeamRequest, LookupEraTeamResponse>(
                new LookupEraTeamRequest { TeamCode = teamCode }, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("me")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetCurrentEraTeam")]
    [ProducesResponseType(typeof(GetCurrentEraTeamResponse), 200)]
    public async Task<IActionResult> GetCurrentAsync(
        [FromQuery] GetCurrentEraTeamRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetCurrentEraTeamRequest, GetCurrentEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{teamId:guid}")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraTeam")]
    [ProducesResponseType(typeof(GetEraTeamResponse), 200)]
    public async Task<IActionResult> GetAsync(
        [FromRoute] GetEraTeamRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraTeamRequest, GetEraTeamResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{teamId:guid}/peers")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraPunchPeers")]
    [ProducesResponseType(typeof(GetEraPunchPeersResponse), 200)]
    public async Task<IActionResult> GetPeersAsync(
        [FromRoute] GetEraPunchPeersRequest message,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraPunchPeersRequest, GetEraPunchPeersResponse>(message, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }
}
