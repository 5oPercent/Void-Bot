using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize]
[EraClientSession]
[ApiController]
[Security]
[Route("api/era/worlds")]
[RequestSizeLimit(4096)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[EnableRateLimiting(EraNetworkingConstants.TeamRateLimitPolicy)]
[OpenApiTagOrder(406)]
public sealed partial class EraWorldsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraWorldsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [SkipPermissionCheck]
    [DescriptionLocalization("RegisterEraWorld")]
    [ProducesResponseType(typeof(RegisterEraWorldResponse), 200)]
    public async Task<IActionResult> RegisterAsync([FromBody] RegisterEraWorldCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<RegisterEraWorldCommand, RegisterEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("current")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetCurrentEraWorld")]
    [ProducesResponseType(typeof(GetCurrentEraWorldResponse), 200)]
    public async Task<IActionResult> GetCurrentAsync(CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetCurrentEraWorldRequest, GetCurrentEraWorldResponse>(new GetCurrentEraWorldRequest(), cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("{worldInstanceId:guid}")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraWorld")]
    [ProducesResponseType(typeof(GetEraWorldResponse), 200)]
    public async Task<IActionResult> GetAsync([FromRoute] Guid worldInstanceId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraWorldRequest, GetEraWorldResponse>(new GetEraWorldRequest { WorldInstanceId = worldInstanceId }, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/ready")]
    [SkipPermissionCheck]
    [DescriptionLocalization("ReadyEraWorld")]
    [ProducesResponseType(typeof(ReadyEraWorldResponse), 200)]
    public async Task<IActionResult> ReadyAsync([FromRoute] Guid worldInstanceId,
        [FromBody] ReadyEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<ReadyEraWorldCommand, ReadyEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/heartbeat")]
    [SkipPermissionCheck]
    [DescriptionLocalization("HeartbeatEraWorld")]
    [ProducesResponseType(typeof(HeartbeatEraWorldResponse), 200)]
    public async Task<IActionResult> HeartbeatAsync([FromRoute] Guid worldInstanceId,
        [FromBody] HeartbeatEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<HeartbeatEraWorldCommand, HeartbeatEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/recover")]
    [SkipPermissionCheck]
    [DescriptionLocalization("RecoverEraWorld")]
    [ProducesResponseType(typeof(RecoverEraWorldResponse), 200)]
    public async Task<IActionResult> RecoverAsync([FromRoute] Guid worldInstanceId,
        [FromBody] RecoverEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<RecoverEraWorldCommand, RecoverEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/join")]
    [SkipPermissionCheck]
    [DescriptionLocalization("JoinEraWorld")]
    [ProducesResponseType(typeof(JoinEraWorldResponse), 200)]
    public async Task<IActionResult> JoinAsync([FromRoute] Guid worldInstanceId,
        [FromBody] JoinEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<JoinEraWorldCommand, JoinEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/leave")]
    [SkipPermissionCheck]
    [DescriptionLocalization("LeaveEraWorld")]
    [ProducesResponseType(typeof(LeaveEraWorldResponse), 200)]
    public async Task<IActionResult> LeaveAsync([FromRoute] Guid worldInstanceId,
        [FromBody] LeaveEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<LeaveEraWorldCommand, LeaveEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/end")]
    [SkipPermissionCheck]
    [DescriptionLocalization("EndEraWorld")]
    [ProducesResponseType(typeof(EndEraWorldResponse), 200)]
    public async Task<IActionResult> EndAsync([FromRoute] Guid worldInstanceId,
        [FromBody] EndEraWorldCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<EndEraWorldCommand, EndEraWorldResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/equipment-restriction")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SetEraWorldEquipmentRestriction")]
    [ProducesResponseType(typeof(SetEraWorldEquipmentRestrictionResponse), 200)]
    public async Task<IActionResult> SetEquipmentRestrictionAsync([FromRoute] Guid worldInstanceId,
        [FromBody] SetEraWorldEquipmentRestrictionCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<SetEraWorldEquipmentRestrictionCommand, SetEraWorldEquipmentRestrictionResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/rewards/ground")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SubmitEraGroundReward")]
    [ProducesResponseType(typeof(SubmitEraGroundRewardResponse), 200)]
    public async Task<IActionResult> SubmitGroundRewardAsync([FromRoute] Guid worldInstanceId,
        [FromBody] SubmitEraGroundRewardCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<SubmitEraGroundRewardCommand, SubmitEraGroundRewardResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/rewards/ground/grant")]
    [DescriptionLocalization("GrantEraGroundReward")]
    [ProducesResponseType(typeof(SubmitEraGroundRewardResponse), 200)]
    public async Task<IActionResult> GrantGroundRewardAsync([FromRoute] Guid worldInstanceId,
        [FromBody] GrantEraGroundRewardCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<GrantEraGroundRewardCommand, SubmitEraGroundRewardResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("{worldInstanceId:guid}/equipment-drops")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraEquipmentDrops")]
    [ProducesResponseType(typeof(GetEraEquipmentDropsResponse), 200)]
    public async Task<IActionResult> GetEquipmentDropsAsync([FromRoute] Guid worldInstanceId, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraEquipmentDropsRequest, GetEraEquipmentDropsResponse>(
            new GetEraEquipmentDropsRequest { WorldInstanceId = worldInstanceId }, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/equipment-drops/destroy")]
    [SkipPermissionCheck]
    [DescriptionLocalization("DestroyEraEquipmentDrops")]
    [ProducesResponseType(typeof(DestroyEraEquipmentDropsResponse), 200)]
    public async Task<IActionResult> DestroyEquipmentDropsAsync([FromRoute] Guid worldInstanceId,
        [FromBody] DestroyEraEquipmentDropsCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<DestroyEraEquipmentDropsCommand, DestroyEraEquipmentDropsResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/equipment-drops/{dropId:guid}/pickup")]
    [SkipPermissionCheck]
    [DescriptionLocalization("PickupEraEquipmentDrop")]
    [ProducesResponseType(typeof(PickupEraEquipmentDropResponse), 200)]
    public async Task<IActionResult> PickupEquipmentDropAsync([FromRoute] Guid worldInstanceId, [FromRoute] Guid dropId,
        [FromBody] PickupEraEquipmentDropCommand command, CancellationToken cancellationToken)
    {
        command.WorldInstanceId = worldInstanceId;
        command.DropId = dropId;
        return Ok(await _mediator.SendAsync<PickupEraEquipmentDropCommand, PickupEraEquipmentDropResponse>(command, cancellationToken).ConfigureAwait(false));
    }
}
