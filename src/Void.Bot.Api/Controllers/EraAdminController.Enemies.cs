using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraAdminController
{
    [HttpGet("enemies")]
    [DescriptionLocalization("ListEraDataEnemy")]
    [ProducesResponseType(typeof(ListEraDataEnemyResponse), 200)]
    public async Task<IActionResult> ListDataEnemyAsync([FromQuery] ListEraDataEnemyRequest request, CancellationToken cancellationToken)
    {
        return Ok(await _mediator.RequestAsync<ListEraDataEnemyRequest, ListEraDataEnemyResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpGet("enemies/{id}")]
    [DescriptionLocalization("GetEraDataEnemy")]
    [ProducesResponseType(typeof(GetEraDataEnemyResponse), 200)]
    public async Task<IActionResult> GetDataEnemyAsync(string id, [FromQuery] GetEraDataEnemyRequest request, CancellationToken cancellationToken)
    {
        request.Id = id;
        return Ok(await _mediator.RequestAsync<GetEraDataEnemyRequest, GetEraDataEnemyResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("enemies/{id}")]
    [DescriptionLocalization("SaveEraDataEnemy")]
    [ProducesResponseType(typeof(SaveEraDataEnemyResponse), 200)]
    public async Task<IActionResult> SaveDataEnemyAsync(string id, [FromBody] SaveEraDataEnemyCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<SaveEraDataEnemyCommand, SaveEraDataEnemyResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("enemies/{id}/delete")]
    [DescriptionLocalization("DeleteEraDataEnemy")]
    [ProducesResponseType(typeof(DeleteEraDataEnemyResponse), 200)]
    public async Task<IActionResult> DeleteDataEnemyAsync(string id, [FromBody] DeleteEraDataEnemyCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        return Ok(await _mediator.SendAsync<DeleteEraDataEnemyCommand, DeleteEraDataEnemyResponse>(command, cancellationToken).ConfigureAwait(false));
    }

}
