using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraCharactersController
{
    [HttpPost("{characterId:guid}/equipment/{equipmentInstanceId:guid}/armor-modifications/select")]
    [SkipPermissionCheck]
    [DescriptionLocalization("SelectEraArmorModification")]
    [ProducesResponseType(typeof(SelectEraArmorModificationResponse), 200)]
    public async Task<IActionResult> SelectArmorModificationAsync(
        [FromRoute] Guid characterId, [FromRoute] Guid equipmentInstanceId,
        [FromBody] SelectEraArmorModificationCommand command, CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        command.EquipmentInstanceId = equipmentInstanceId;
        var response = await _mediator.SendAsync<SelectEraArmorModificationCommand, SelectEraArmorModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
