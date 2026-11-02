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
    [DescriptionLocalization("GetEraSkillDraft")]
    [ProducesResponseType(typeof(GetEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> GetDraftStatusAsync([FromQuery] GetEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraResonanceDraftRequest, GetEraResonanceDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/initialize")]
    [DescriptionLocalization("InitializeEraSkillDraft")]
    [ProducesResponseType(typeof(InitializeEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> InitializeDraftAsync([FromBody] InitializeEraResonanceDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<InitializeEraResonanceDraftCommand, InitializeEraResonanceDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/export")]
    [DescriptionLocalization("ExportEraSkillDraft")]
    [ProducesResponseType(typeof(ExportEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> ExportDraftAsync([FromQuery] ExportEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ExportEraResonanceDraftRequest, ExportEraResonanceDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems")]
    [DescriptionLocalization("ListEraDraftSystem")]
    [ProducesResponseType(typeof(ListEraDraftLatticeResponse), 200)]
    public async Task<IActionResult> ListDraftLatticeAsync([FromQuery] ListEraDraftLatticeRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftLatticeRequest, ListEraDraftLatticeResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems/{id}")]
    [DescriptionLocalization("GetEraDraftSystem")]
    [ProducesResponseType(typeof(GetEraDraftLatticeResponse), 200)]
    public async Task<IActionResult> GetDraftLatticeAsync(string id, [FromQuery] GetEraDraftLatticeRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftLatticeRequest, GetEraDraftLatticeResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}")]
    [DescriptionLocalization("SaveEraDraftSystem")]
    [ProducesResponseType(typeof(SaveEraDraftLatticeResponse), 200)]
    public async Task<IActionResult> SaveDraftLatticeAsync(string id, [FromBody] SaveEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftLatticeCommand, SaveEraDraftLatticeResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftSystem")]
    [ProducesResponseType(typeof(DeleteEraDraftLatticeResponse), 200)]
    public async Task<IActionResult> DeleteDraftLatticeAsync(string id, [FromBody] DeleteEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftLatticeCommand, DeleteEraDraftLatticeResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftSystem")]
    [ProducesResponseType(typeof(RestoreEraDraftLatticeResponse), 200)]
    public async Task<IActionResult> RestoreDraftLatticeAsync(string id, [FromBody] RestoreEraDraftLatticeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftLatticeCommand, RestoreEraDraftLatticeResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills")]
    [DescriptionLocalization("ListEraDraftSkill")]
    [ProducesResponseType(typeof(ListEraDraftResonanceResponse), 200)]
    public async Task<IActionResult> ListDraftResonanceAsync([FromQuery] ListEraDraftResonanceRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftResonanceRequest, ListEraDraftResonanceResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills/{id}")]
    [DescriptionLocalization("GetEraDraftSkill")]
    [ProducesResponseType(typeof(GetEraDraftResonanceResponse), 200)]
    public async Task<IActionResult> GetDraftResonanceAsync(string id, [FromQuery] GetEraDraftResonanceRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftResonanceRequest, GetEraDraftResonanceResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}")]
    [DescriptionLocalization("SaveEraDraftSkill")]
    [ProducesResponseType(typeof(SaveEraDraftResonanceResponse), 200)]
    public async Task<IActionResult> SaveDraftResonanceAsync(string id, [FromBody] SaveEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftResonanceCommand, SaveEraDraftResonanceResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftSkill")]
    [ProducesResponseType(typeof(DeleteEraDraftResonanceResponse), 200)]
    public async Task<IActionResult> DeleteDraftResonanceAsync(string id, [FromBody] DeleteEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftResonanceCommand, DeleteEraDraftResonanceResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftSkill")]
    [ProducesResponseType(typeof(RestoreEraDraftResonanceResponse), 200)]
    public async Task<IActionResult> RestoreDraftResonanceAsync(string id, [FromBody] RestoreEraDraftResonanceCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftResonanceCommand, RestoreEraDraftResonanceResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("buffs")]
    [DescriptionLocalization("ListEraDraftBuff")]
    [ProducesResponseType(typeof(ListEraDraftBuffResponse), 200)]
    public async Task<IActionResult> ListDraftBuffAsync([FromQuery] ListEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftBuffRequest, ListEraDraftBuffResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("buffs/{id}")]
    [DescriptionLocalization("GetEraDraftBuff")]
    [ProducesResponseType(typeof(GetEraDraftBuffResponse), 200)]
    public async Task<IActionResult> GetDraftBuffAsync(string id, [FromQuery] GetEraDraftBuffRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftBuffRequest, GetEraDraftBuffResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("buffs/{id}")]
    [DescriptionLocalization("SaveEraDraftBuff")]
    [ProducesResponseType(typeof(SaveEraDraftBuffResponse), 200)]
    public async Task<IActionResult> SaveDraftBuffAsync(string id, [FromBody] SaveEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftBuffCommand, SaveEraDraftBuffResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("buffs/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftBuff")]
    [ProducesResponseType(typeof(DeleteEraDraftBuffResponse), 200)]
    public async Task<IActionResult> DeleteDraftBuffAsync(string id, [FromBody] DeleteEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftBuffCommand, DeleteEraDraftBuffResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("buffs/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftBuff")]
    [ProducesResponseType(typeof(RestoreEraDraftBuffResponse), 200)]
    public async Task<IActionResult> RestoreDraftBuffAsync(string id, [FromBody] RestoreEraDraftBuffCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftBuffCommand, RestoreEraDraftBuffResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/settings")]
    [DescriptionLocalization("GetEraDraftSettings")]
    [ProducesResponseType(typeof(GetEraDraftSettingsResponse), 200)]
    public async Task<IActionResult> GetDraftSettingsAsync([FromQuery] GetEraDraftSettingsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDraftSettingsRequest, GetEraDraftSettingsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/settings")]
    [DescriptionLocalization("SaveEraDraftSettings")]
    [ProducesResponseType(typeof(SaveEraDraftSettingsResponse), 200)]
    public async Task<IActionResult> SaveDraftSettingsAsync([FromBody] SaveEraDraftSettingsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<SaveEraDraftSettingsCommand, SaveEraDraftSettingsResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/validate")]
    [DescriptionLocalization("ValidateEraSkillDraft")]
    [ProducesResponseType(typeof(ValidateEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> ValidateDraftAsync([FromBody] ValidateEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ValidateEraResonanceDraftRequest, ValidateEraResonanceDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/publish-preview")]
    [DescriptionLocalization("PreviewEraSkillDraft")]
    [ProducesResponseType(typeof(PreviewEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> PreviewDraftAsync([FromBody] PreviewEraResonanceDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<PreviewEraResonanceDraftRequest, PreviewEraResonanceDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/references")]
    [DescriptionLocalization("GetEraDraftReferences")]
    [ProducesResponseType(typeof(GetEraDraftReferencesResponse), 200)]
    public async Task<IActionResult> GetDraftReferencesAsync([FromQuery] GetEraDraftReferencesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDraftReferencesRequest, GetEraDraftReferencesResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/import")]
    [DescriptionLocalization("ImportEraSkillDraft")]
    [ProducesResponseType(typeof(ImportEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> ImportDraftAsync([FromBody] ImportEraResonanceDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<ImportEraResonanceDraftCommand, ImportEraResonanceDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/publish")]
    [DescriptionLocalization("PublishEraSkillDraft")]
    [ProducesResponseType(typeof(PublishEraResonanceDraftResponse), 200)]
    public async Task<IActionResult> PublishDraftAsync([FromBody] PublishEraResonanceDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<PublishEraResonanceDraftCommand, PublishEraResonanceDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/history")]
    [DescriptionLocalization("ListEraSkillPublications")]
    [ProducesResponseType(typeof(ListEraResonancePublicationsResponse), 200)]
    public async Task<IActionResult> ListPublicationsAsync([FromQuery] ListEraResonancePublicationsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraResonancePublicationsRequest, ListEraResonancePublicationsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/history/{version:long}")]
    [DescriptionLocalization("GetEraSkillPublication")]
    [ProducesResponseType(typeof(GetEraResonancePublicationResponse), 200)]
    public async Task<IActionResult> GetPublicationAsync(long version, [FromQuery] GetEraResonancePublicationRequest request, CancellationToken cancellationToken)
    {
        request.Version = version;
        return Ok(await _mediator.RequestAsync<GetEraResonancePublicationRequest, GetEraResonancePublicationResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/history/{version:long}/restore")]
    [DescriptionLocalization("RestoreEraDraftPublication")]
    [ProducesResponseType(typeof(RestoreEraDraftPublicationResponse), 200)]
    public async Task<IActionResult> RestorePublicationToDraftAsync(long version, [FromBody] RestoreEraDraftPublicationCommand command, CancellationToken cancellationToken)
    {
        command.Version = version;
        return Ok(await _mediator.SendAsync<RestoreEraDraftPublicationCommand, RestoreEraDraftPublicationResponse>(command, cancellationToken).ConfigureAwait(false));
    }
}
