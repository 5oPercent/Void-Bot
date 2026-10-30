using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraAdminController
{
    [HttpGet("equipment/weapon-frameworks")]
    [DescriptionLocalization("GetEraWeaponFrameworks")]
    [ProducesResponseType(typeof(GetEraWeaponFrameworksResponse), 200)]
    public async Task<IActionResult> GetWeaponFrameworksAsync([FromQuery] GetEraWeaponFrameworksRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponFrameworksRequest, GetEraWeaponFrameworksResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-frameworks/{id:guid}")]
    [DescriptionLocalization("GetEraWeaponFramework")]
    [ProducesResponseType(typeof(GetEraWeaponFrameworkResponse), 200)]
    public async Task<IActionResult> GetWeaponFrameworkAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponFrameworkRequest, GetEraWeaponFrameworkResponse>(new GetEraWeaponFrameworkRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-frameworks")]
    [DescriptionLocalization("SaveEraWeaponFramework")]
    [ProducesResponseType(typeof(SaveEraWeaponFrameworkResponse), 200)]
    public async Task<IActionResult> CreateWeaponFrameworkAsync([FromBody] SaveEraWeaponFrameworkCommand command, CancellationToken cancellationToken)
    {
        if (command.Framework != null) command.Framework.Id = Guid.NewGuid();
        var response = await _mediator.SendAsync<SaveEraWeaponFrameworkCommand, SaveEraWeaponFrameworkResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-frameworks/{id:guid}")]
    [DescriptionLocalization("SaveEraWeaponFramework")]
    [ProducesResponseType(typeof(SaveEraWeaponFrameworkResponse), 200)]
    public async Task<IActionResult> SaveWeaponFrameworkAsync([FromRoute] Guid id, [FromBody] SaveEraWeaponFrameworkCommand command, CancellationToken cancellationToken)
    {
        if (command.Framework != null) command.Framework.Id = id;
        var response = await _mediator.SendAsync<SaveEraWeaponFrameworkCommand, SaveEraWeaponFrameworkResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-frameworks/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraWeaponFramework")]
    [ProducesResponseType(typeof(DeleteEraWeaponFrameworkResponse), 200)]
    public async Task<IActionResult> DeleteWeaponFrameworkAsync([FromRoute] Guid id, [FromBody] DeleteEraWeaponFrameworkCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraWeaponFrameworkCommand, DeleteEraWeaponFrameworkResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-frameworks/{id:guid}/disable")]
    [DescriptionLocalization("DisableEraWeaponFramework")]
    [ProducesResponseType(typeof(DisableEraWeaponFrameworkResponse), 200)]
    public async Task<IActionResult> DisableWeaponFrameworkAsync([FromRoute] Guid id, [FromBody] DisableEraWeaponFrameworkCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DisableEraWeaponFrameworkCommand, DisableEraWeaponFrameworkResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

}
