using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraCharactersController
{
    [HttpPost("{characterId:guid}/equipment/{equipmentId:guid}/armor-modifications/select")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SelectEraArmorModification")]
    [ProducesResponseType(typeof(SelectEraArmorModificationResponse), 200)]
    public async Task<IActionResult> SelectArmorModificationAsync(
        [FromRoute] Guid characterId, [FromRoute] Guid equipmentId,
        [FromBody] SelectEraArmorModificationCommand command, CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        command.EquipmentId = equipmentId;
        var response = await _mediator.SendAsync<SelectEraArmorModificationCommand, SelectEraArmorModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
