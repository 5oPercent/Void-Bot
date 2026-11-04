using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

[Authorize]
[EraClientSession]
[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
[Security]
[Route("api/era/admin")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(499)]
public sealed partial class EraAdminController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraAdminController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("players/{playerId:guid}")]
    [RequestSizeLimit(16384)]
    [DescriptionLocalization("GetEraAdminPlayer")]
    [ProducesResponseType(typeof(GetEraAdminPlayerResponse), 200)]
    public async Task<IActionResult> GetAdminPlayerAsync(
        [FromRoute] GetEraAdminPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraAdminPlayerRequest, GetEraAdminPlayerResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("config")]
    [RequestSizeLimit(16384)]
    [DescriptionLocalization("UpdateEraConfig")]
    [ProducesResponseType(typeof(UpdateEraConfigResponse), 200)]
    public async Task<IActionResult> UpdateConfigAsync(
        [FromBody] UpdateEraConfigCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<UpdateEraConfigCommand, UpdateEraConfigResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("players/{playerId:guid}/warehouse-capacity")]
    [RequestSizeLimit(16384)]
    [DescriptionLocalization("SetEraWarehouseCapacity")]
    [ProducesResponseType(typeof(SetEraWarehouseCapacityResponse), 200)]
    public async Task<IActionResult> SetCapacityAsync(
        [FromRoute] Guid playerId,
        [FromBody] SetEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        command.PlayerId = playerId;
        var response = await _mediator
            .SendAsync<SetEraWarehouseCapacityCommand, SetEraWarehouseCapacityResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("players/{playerId:guid}/warehouse-capacity-adjustments")]
    [RequestSizeLimit(16384)]
    [DescriptionLocalization("AdjustEraWarehouseCapacity")]
    [ProducesResponseType(typeof(AdjustEraWarehouseCapacityResponse), 200)]
    public async Task<IActionResult> AdjustCapacityAsync(
        [FromRoute] Guid playerId,
        [FromBody] AdjustEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        command.PlayerId = playerId;
        var response = await _mediator
            .SendAsync<AdjustEraWarehouseCapacityCommand, AdjustEraWarehouseCapacityResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("characters/{characterId:guid}/learned-skills")]
    [RequestSizeLimit(16384)]
    [DescriptionLocalization("GrantEraLearnedSkill")]
    [ProducesResponseType(typeof(GrantEraLearnedResonanceResponse), 200)]
    public async Task<IActionResult> GrantAsync(
        [FromRoute] Guid characterId,
        [FromBody] GrantEraLearnedResonanceCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<GrantEraLearnedResonanceCommand, GrantEraLearnedResonanceResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("configuration")]
    [DescriptionLocalization("GetEraSkillData")]
    [ProducesResponseType(typeof(GetEraResonanceDataResponse), 200)]
    public async Task<IActionResult> GetDataStatusAsync([FromQuery] GetEraResonanceDataRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraResonanceDataRequest, GetEraResonanceDataResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/export")]
    [DescriptionLocalization("ExportEraSkillData")]
    [ProducesResponseType(typeof(ExportEraResonanceDataResponse), 200)]
    public async Task<IActionResult> ExportDataAsync([FromQuery] ExportEraResonanceDataRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ExportEraResonanceDataRequest, ExportEraResonanceDataResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems")]
    [DescriptionLocalization("ListEraDataSystem")]
    [ProducesResponseType(typeof(ListEraDataLatticeResponse), 200)]
    public async Task<IActionResult> ListDataLatticeAsync([FromQuery] ListEraDataLatticeRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDataLatticeRequest, ListEraDataLatticeResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems/{id}")]
    [DescriptionLocalization("GetEraDataSystem")]
    [ProducesResponseType(typeof(GetEraDataLatticeResponse), 200)]
    public async Task<IActionResult> GetDataLatticeAsync(string id, [FromQuery] GetEraDataLatticeRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDataLatticeRequest, GetEraDataLatticeResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}")]
    [DescriptionLocalization("SaveEraDataSystem")]
    [ProducesResponseType(typeof(SaveEraDataLatticeResponse), 200)]
    public async Task<IActionResult> SaveDataLatticeAsync(string id, [FromBody] SaveEraDataLatticeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDataLatticeCommand, SaveEraDataLatticeResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}/delete")]
    [DescriptionLocalization("DeleteEraDataSystem")]
    [ProducesResponseType(typeof(DeleteEraDataLatticeResponse), 200)]
    public async Task<IActionResult> DeleteDataLatticeAsync(string id, [FromBody] DeleteEraDataLatticeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDataLatticeCommand, DeleteEraDataLatticeResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills")]
    [DescriptionLocalization("ListEraDataSkill")]
    [ProducesResponseType(typeof(ListEraDataResonanceResponse), 200)]
    public async Task<IActionResult> ListDataResonanceAsync([FromQuery] ListEraDataResonanceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDataResonanceRequest, ListEraDataResonanceResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills/{id}")]
    [DescriptionLocalization("GetEraDataSkill")]
    [ProducesResponseType(typeof(GetEraDataResonanceResponse), 200)]
    public async Task<IActionResult> GetDataResonanceAsync(string id, [FromQuery] GetEraDataResonanceRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDataResonanceRequest, GetEraDataResonanceResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}")]
    [DescriptionLocalization("SaveEraDataSkill")]
    [ProducesResponseType(typeof(SaveEraDataResonanceResponse), 200)]
    public async Task<IActionResult> SaveDataResonanceAsync(string id, [FromBody] SaveEraDataResonanceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDataResonanceCommand, SaveEraDataResonanceResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}/delete")]
    [DescriptionLocalization("DeleteEraDataSkill")]
    [ProducesResponseType(typeof(DeleteEraDataResonanceResponse), 200)]
    public async Task<IActionResult> DeleteDataResonanceAsync(string id, [FromBody] DeleteEraDataResonanceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDataResonanceCommand, DeleteEraDataResonanceResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("buffs")]
    [DescriptionLocalization("ListEraDataBuff")]
    [ProducesResponseType(typeof(ListEraDataBuffResponse), 200)]
    public async Task<IActionResult> ListDataBuffAsync([FromQuery] ListEraDataBuffRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDataBuffRequest, ListEraDataBuffResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("buffs/{id}")]
    [DescriptionLocalization("GetEraDataBuff")]
    [ProducesResponseType(typeof(GetEraDataBuffResponse), 200)]
    public async Task<IActionResult> GetDataBuffAsync(string id, [FromQuery] GetEraDataBuffRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDataBuffRequest, GetEraDataBuffResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("buffs/{id}")]
    [DescriptionLocalization("SaveEraDataBuff")]
    [ProducesResponseType(typeof(SaveEraDataBuffResponse), 200)]
    public async Task<IActionResult> SaveDataBuffAsync(string id, [FromBody] SaveEraDataBuffCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDataBuffCommand, SaveEraDataBuffResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("buffs/{id}/delete")]
    [DescriptionLocalization("DeleteEraDataBuff")]
    [ProducesResponseType(typeof(DeleteEraDataBuffResponse), 200)]
    public async Task<IActionResult> DeleteDataBuffAsync(string id, [FromBody] DeleteEraDataBuffCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDataBuffCommand, DeleteEraDataBuffResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/settings")]
    [DescriptionLocalization("GetEraDataSettings")]
    [ProducesResponseType(typeof(GetEraDataSettingsResponse), 200)]
    public async Task<IActionResult> GetDataSettingsAsync([FromQuery] GetEraDataSettingsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDataSettingsRequest, GetEraDataSettingsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/settings")]
    [DescriptionLocalization("SaveEraDataSettings")]
    [ProducesResponseType(typeof(SaveEraDataSettingsResponse), 200)]
    public async Task<IActionResult> SaveDataSettingsAsync([FromBody] SaveEraDataSettingsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<SaveEraDataSettingsCommand, SaveEraDataSettingsResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/references")]
    [DescriptionLocalization("GetEraDataReferences")]
    [ProducesResponseType(typeof(GetEraDataReferencesResponse), 200)]
    public async Task<IActionResult> GetDataReferencesAsync([FromQuery] GetEraDataReferencesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDataReferencesRequest, GetEraDataReferencesResponse>(request, cancellationToken).ConfigureAwait(false));
    }

}
