using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraCharactersController
{
    [HttpPost("{characterId:guid}/equipment/{equipmentId:guid}/modifications/select")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SelectEraWeaponModification")]
    [ProducesResponseType(typeof(SelectEraWeaponModificationResponse), 200)]
    public async Task<IActionResult> SelectWeaponModificationAsync(
        [FromRoute] Guid characterId, [FromRoute] Guid equipmentId,
        [FromBody] SelectEraWeaponModificationCommand command, CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        command.EquipmentId = equipmentId;
        var response = await _mediator.SendAsync<SelectEraWeaponModificationCommand, SelectEraWeaponModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
