using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraAdminController
{
    [HttpGet("enemies")]
    [DescriptionLocalization("ListEraDraftEnemy")]
    [ProducesResponseType(typeof(ListEraDraftEnemyResponse), 200)]
    public async Task<IActionResult> ListDraftEnemyAsync([FromQuery] ListEraDraftEnemyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDraftEnemyRequest, ListEraDraftEnemyResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("enemies/{id}")]
    [DescriptionLocalization("GetEraDraftEnemy")]
    [ProducesResponseType(typeof(GetEraDraftEnemyResponse), 200)]
    public async Task<IActionResult> GetDraftEnemyAsync(string id, [FromQuery] GetEraDraftEnemyRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDraftEnemyRequest, GetEraDraftEnemyResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("enemies/{id}")]
    [DescriptionLocalization("SaveEraDraftEnemy")]
    [ProducesResponseType(typeof(SaveEraDraftEnemyResponse), 200)]
    public async Task<IActionResult> SaveDraftEnemyAsync(string id, [FromBody] SaveEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDraftEnemyCommand, SaveEraDraftEnemyResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("enemies/{id}/delete")]
    [DescriptionLocalization("DeleteEraDraftEnemy")]
    [ProducesResponseType(typeof(DeleteEraDraftEnemyResponse), 200)]
    public async Task<IActionResult> DeleteDraftEnemyAsync(string id, [FromBody] DeleteEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDraftEnemyCommand, DeleteEraDraftEnemyResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("enemies/{id}/restore")]
    [DescriptionLocalization("RestoreEraDraftEnemy")]
    [ProducesResponseType(typeof(RestoreEraDraftEnemyResponse), 200)]
    public async Task<IActionResult> RestoreDraftEnemyAsync(string id, [FromBody] RestoreEraDraftEnemyCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<RestoreEraDraftEnemyCommand, RestoreEraDraftEnemyResponse>(command, cancellationToken).ConfigureAwait(false));
    }

}
