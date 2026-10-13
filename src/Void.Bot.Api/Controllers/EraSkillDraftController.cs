using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;
/// <summary>体系、技能和 BUFF 的管理员草稿编辑，保存不改变游戏发布配置。</summary>
[Authorize]
[Security]
[ApiController]
[EraClientSession]
[Route("api/era/admin/skills/draft")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(405)]
public sealed class EraSkillDraftController : ControllerBase
{
    private readonly IMediator _mediator;
    public EraSkillDraftController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [DescriptionLocalization("GetEraSkillDraft")]
    [ProducesResponseType(typeof(GetEraSkillDraftResponse), 200)]
    public async Task<IActionResult> GetDraftStatusAsync([FromQuery] GetEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraSkillDraftRequest, GetEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("initialize")]
    [DescriptionLocalization("InitializeEraSkillDraft")]
    [ProducesResponseType(typeof(InitializeEraSkillDraftResponse), 200)]
    public async Task<IActionResult> InitializeDraftAsync([FromBody] InitializeEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<InitializeEraSkillDraftCommand, InitializeEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("export")]
    [DescriptionLocalization("ExportEraSkillDraft")]
    [ProducesResponseType(typeof(ExportEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ExportDraftAsync([FromQuery] ExportEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ExportEraSkillDraftRequest, ExportEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("systems")]
    [DescriptionLocalization("ListEraDraftSystem")]
    [ProducesResponseType(typeof(ListEraDraftSystemResponse), 200)]
    public async Task<IActionResult> ListDraftSystemAsync([FromQuery] ListEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftSystemRequest, ListEraDraftSystemResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("systems/{id}")]
    [DescriptionLocalization("GetEraDraftSystem")]
    [ProducesResponseType(typeof(GetEraDraftSystemResponse), 200)]
    public async Task<IActionResult> GetDraftSystemAsync(string id, [FromQuery] GetEraDraftSystemRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftSystemRequest, GetEraDraftSystemResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("systems/{id}")]
    [DescriptionLocalization("SaveEraDraftSystem")]
    [ProducesResponseType(typeof(SaveEraDraftSystemResponse), 200)]
    public async Task<IActionResult> SaveDraftSystemAsync(string id, [FromBody] SaveEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftSystemCommand, SaveEraDraftSystemResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("systems/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftSystem")]
    [ProducesResponseType(typeof(DeleteEraDraftSystemResponse), 200)]
    public async Task<IActionResult> DeleteDraftSystemAsync(string id, [FromBody] DeleteEraDraftSystemCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftSystemCommand, DeleteEraDraftSystemResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("systems/{id}/restore")]
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

    [HttpGet("settings")]
    [DescriptionLocalization("GetEraDraftSettings")]
    [ProducesResponseType(typeof(GetEraDraftSettingsResponse), 200)]
    public async Task<IActionResult> GetDraftSettingsAsync([FromQuery] GetEraDraftSettingsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDraftSettingsRequest, GetEraDraftSettingsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("settings")]
    [DescriptionLocalization("SaveEraDraftSettings")]
    [ProducesResponseType(typeof(SaveEraDraftSettingsResponse), 200)]
    public async Task<IActionResult> SaveDraftSettingsAsync([FromBody] SaveEraDraftSettingsCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<SaveEraDraftSettingsCommand, SaveEraDraftSettingsResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("validate")]
    [DescriptionLocalization("ValidateEraSkillDraft")]
    [ProducesResponseType(typeof(ValidateEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ValidateDraftAsync([FromBody] ValidateEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ValidateEraSkillDraftRequest, ValidateEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("publish-preview")]
    [DescriptionLocalization("PreviewEraSkillDraft")]
    [ProducesResponseType(typeof(PreviewEraSkillDraftResponse), 200)]
    public async Task<IActionResult> PreviewDraftAsync([FromBody] PreviewEraSkillDraftRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<PreviewEraSkillDraftRequest, PreviewEraSkillDraftResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("references")]
    [DescriptionLocalization("GetEraDraftReferences")]
    [ProducesResponseType(typeof(GetEraDraftReferencesResponse), 200)]
    public async Task<IActionResult> GetDraftReferencesAsync([FromQuery] GetEraDraftReferencesRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<GetEraDraftReferencesRequest, GetEraDraftReferencesResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("import")]
    [DescriptionLocalization("ImportEraSkillDraft")]
    [ProducesResponseType(typeof(ImportEraSkillDraftResponse), 200)]
    public async Task<IActionResult> ImportDraftAsync([FromBody] ImportEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<ImportEraSkillDraftCommand, ImportEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("publish")]
    [DescriptionLocalization("PublishEraSkillDraft")]
    [ProducesResponseType(typeof(PublishEraSkillDraftResponse), 200)]
    public async Task<IActionResult> PublishDraftAsync([FromBody] PublishEraSkillDraftCommand command, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.SendAsync<PublishEraSkillDraftCommand, PublishEraSkillDraftResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("history")]
    [DescriptionLocalization("ListEraSkillPublications")]
    [ProducesResponseType(typeof(ListEraSkillPublicationsResponse), 200)]
    public async Task<IActionResult> ListPublicationsAsync([FromQuery] ListEraSkillPublicationsRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraSkillPublicationsRequest, ListEraSkillPublicationsResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("history/{version:long}")]
    [DescriptionLocalization("GetEraSkillPublication")]
    [ProducesResponseType(typeof(GetEraSkillPublicationResponse), 200)]
    public async Task<IActionResult> GetPublicationAsync(long version, [FromQuery] GetEraSkillPublicationRequest request, CancellationToken cancellationToken)
    {
        request.Version = version;
        return Ok(await _mediator.RequestAsync<GetEraSkillPublicationRequest, GetEraSkillPublicationResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("history/{version:long}/restore")]
    [DescriptionLocalization("RestoreEraDraftPublication")]
    [ProducesResponseType(typeof(RestoreEraDraftPublicationResponse), 200)]
    public async Task<IActionResult> RestorePublicationToDraftAsync(long version, [FromBody] RestoreEraDraftPublicationCommand command, CancellationToken cancellationToken)
    {
        command.Version = version;
        return Ok(await _mediator.SendAsync<RestoreEraDraftPublicationCommand, RestoreEraDraftPublicationResponse>(command, cancellationToken).ConfigureAwait(false));
    }
}
