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
public sealed class EraAdminController : ControllerBase
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
    [ProducesResponseType(typeof(GrantEraLearnedSkillResponse), 200)]
    public async Task<IActionResult> GrantAsync(
        [FromRoute] Guid characterId,
        [FromBody] GrantEraLearnedSkillCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<GrantEraLearnedSkillCommand, GrantEraLearnedSkillResponse>(command, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet("configuration")]
    [DescriptionLocalization("GetEraSkillDraft")]
    [ProducesResponseType(typeof(GetEraSkillDraftResponse), 200)]
    public async Task<IActionResult> GetDraftStatusAsync([FromQuery] GetEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraSkillDraftRequest, GetEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/initialize")]
    [DescriptionLocalization("InitializeEraSkillDraft")]
    [ProducesResponseType(typeof(InitializeEraSkillDraftResponse), 200)]
    public async Task<IActionResult> InitializeDraftAsync([FromBody] InitializeEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<InitializeEraSkillDraftCommand, InitializeEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/export")]
    [DescriptionLocalization("ExportEraSkillDraft")]
    [ProducesResponseType(typeof(ExportEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ExportDraftAsync([FromQuery] ExportEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ExportEraSkillDraftRequest, ExportEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems")]
    [DescriptionLocalization("ListEraDraftSystem")]
    [ProducesResponseType(typeof(ListEraDraftSystemResponse), 200)]
    public async Task<IActionResult> ListDraftSystemAsync([FromQuery] ListEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftSystemRequest, ListEraDraftSystemResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("ability-systems/{id}")]
    [DescriptionLocalization("GetEraDraftSystem")]
    [ProducesResponseType(typeof(GetEraDraftSystemResponse), 200)]
    public async Task<IActionResult> GetDraftSystemAsync(string id, [FromQuery] GetEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftSystemRequest, GetEraDraftSystemResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}")]
    [DescriptionLocalization("SaveEraDraftSystem")]
    [ProducesResponseType(typeof(SaveEraDraftSystemResponse), 200)]
    public async Task<IActionResult> SaveDraftSystemAsync(string id, [FromBody] SaveEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftSystemCommand, SaveEraDraftSystemResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftSystem")]
    [ProducesResponseType(typeof(DeleteEraDraftSystemResponse), 200)]
    public async Task<IActionResult> DeleteDraftSystemAsync(string id, [FromBody] DeleteEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftSystemCommand, DeleteEraDraftSystemResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("ability-systems/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftSystem")]
    [ProducesResponseType(typeof(RestoreEraDraftSystemResponse), 200)]
    public async Task<IActionResult> RestoreDraftSystemAsync(string id, [FromBody] RestoreEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftSystemCommand, RestoreEraDraftSystemResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills")]
    [DescriptionLocalization("ListEraDraftSkill")]
    [ProducesResponseType(typeof(ListEraDraftSkillResponse), 200)]
    public async Task<IActionResult> ListDraftSkillAsync([FromQuery] ListEraDraftSkillRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftSkillRequest, ListEraDraftSkillResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("skills/{id}")]
    [DescriptionLocalization("GetEraDraftSkill")]
    [ProducesResponseType(typeof(GetEraDraftSkillResponse), 200)]
    public async Task<IActionResult> GetDraftSkillAsync(string id, [FromQuery] GetEraDraftSkillRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftSkillRequest, GetEraDraftSkillResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}")]
    [DescriptionLocalization("SaveEraDraftSkill")]
    [ProducesResponseType(typeof(SaveEraDraftSkillResponse), 200)]
    public async Task<IActionResult> SaveDraftSkillAsync(string id, [FromBody] SaveEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftSkillCommand, SaveEraDraftSkillResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftSkill")]
    [ProducesResponseType(typeof(DeleteEraDraftSkillResponse), 200)]
    public async Task<IActionResult> DeleteDraftSkillAsync(string id, [FromBody] DeleteEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftSkillCommand, DeleteEraDraftSkillResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("skills/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftSkill")]
    [ProducesResponseType(typeof(RestoreEraDraftSkillResponse), 200)]
    public async Task<IActionResult> RestoreDraftSkillAsync(string id, [FromBody] RestoreEraDraftSkillCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftSkillCommand, RestoreEraDraftSkillResponse>(command, cancellationToken).ConfigureAwait(false));
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
    [ProducesResponseType(typeof(ValidateEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ValidateDraftAsync([FromBody] ValidateEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ValidateEraSkillDraftRequest, ValidateEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/publish-preview")]
    [DescriptionLocalization("PreviewEraSkillDraft")]
    [ProducesResponseType(typeof(PreviewEraSkillDraftResponse), 200)]
    public async Task<IActionResult> PreviewDraftAsync([FromBody] PreviewEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<PreviewEraSkillDraftRequest, PreviewEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
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
    [ProducesResponseType(typeof(ImportEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ImportDraftAsync([FromBody] ImportEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<ImportEraSkillDraftCommand, ImportEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("configuration/publish")]
    [DescriptionLocalization("PublishEraSkillDraft")]
    [ProducesResponseType(typeof(PublishEraSkillDraftResponse), 200)]
    public async Task<IActionResult> PublishDraftAsync([FromBody] PublishEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<PublishEraSkillDraftCommand, PublishEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/history")]
    [DescriptionLocalization("ListEraSkillPublications")]
    [ProducesResponseType(typeof(ListEraSkillPublicationsResponse), 200)]
    public async Task<IActionResult> ListPublicationsAsync([FromQuery] ListEraSkillPublicationsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraSkillPublicationsRequest, ListEraSkillPublicationsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("configuration/history/{version:long}")]
    [DescriptionLocalization("GetEraSkillPublication")]
    [ProducesResponseType(typeof(GetEraSkillPublicationResponse), 200)]
    public async Task<IActionResult> GetPublicationAsync(long version, [FromQuery] GetEraSkillPublicationRequest request, CancellationToken cancellationToken)
    {
        request.Version = version;
        return Ok(await _mediator.RequestAsync<GetEraSkillPublicationRequest, GetEraSkillPublicationResponse>(request, cancellationToken).ConfigureAwait(false));
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
