using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraAdminController
{
    [HttpGet("equipment/weapon-templates")]
    [DescriptionLocalization("GetEraWeaponTemplates")]
    [ProducesResponseType(typeof(GetEraWeaponTemplatesResponse), 200)]
    public async Task<IActionResult> GetWeaponTemplatesAsync([FromQuery] GetEraWeaponTemplatesRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponTemplatesRequest, GetEraWeaponTemplatesResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-templates/{id:guid}")]
    [DescriptionLocalization("GetEraWeaponTemplate")]
    [ProducesResponseType(typeof(GetEraWeaponTemplateResponse), 200)]
    public async Task<IActionResult> GetWeaponTemplateAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponTemplateRequest, GetEraWeaponTemplateResponse>(new GetEraWeaponTemplateRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-templates")]
    [DescriptionLocalization("SaveEraWeaponTemplate")]
    [ProducesResponseType(typeof(SaveEraWeaponTemplateResponse), 200)]
    public async Task<IActionResult> CreateWeaponTemplateAsync([FromBody] SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-templates/{id:guid}")]
    [DescriptionLocalization("SaveEraWeaponTemplate")]
    [ProducesResponseType(typeof(SaveEraWeaponTemplateResponse), 200)]
    public async Task<IActionResult> SaveWeaponTemplateAsync([FromRoute] Guid id, [FromBody] SaveEraWeaponTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Template != null) command.Template.Id = id;
        var response = await _mediator.SendAsync<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-templates/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraWeaponTemplate")]
    [ProducesResponseType(typeof(DeleteEraWeaponTemplateResponse), 200)]
    public async Task<IActionResult> DeleteWeaponTemplateAsync([FromRoute] Guid id, [FromBody] DeleteEraWeaponTemplateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraWeaponTemplateCommand, DeleteEraWeaponTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-modifications")]
    [DescriptionLocalization("GetEraWeaponModifications")]
    [ProducesResponseType(typeof(GetEraWeaponModificationsResponse), 200)]
    public async Task<IActionResult> GetWeaponModificationsAsync([FromQuery] GetEraWeaponModificationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponModificationsRequest, GetEraWeaponModificationsResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-modifications/{id:guid}")]
    [DescriptionLocalization("GetEraWeaponModification")]
    [ProducesResponseType(typeof(GetEraWeaponModificationResponse), 200)]
    public async Task<IActionResult> GetWeaponModificationAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponModificationRequest, GetEraWeaponModificationResponse>(new GetEraWeaponModificationRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-modifications")]
    [DescriptionLocalization("SaveEraWeaponModification")]
    [ProducesResponseType(typeof(SaveEraWeaponModificationResponse), 200)]
    public async Task<IActionResult> CreateWeaponModificationAsync([FromBody] SaveEraWeaponModificationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraWeaponModificationCommand, SaveEraWeaponModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-modifications/{id:guid}")]
    [DescriptionLocalization("SaveEraWeaponModification")]
    [ProducesResponseType(typeof(SaveEraWeaponModificationResponse), 200)]
    public async Task<IActionResult> SaveWeaponModificationAsync([FromRoute] Guid id, [FromBody] SaveEraWeaponModificationCommand command, CancellationToken cancellationToken)
    {
        if (command.Modification != null) command.Modification.Id = id;
        var response = await _mediator.SendAsync<SaveEraWeaponModificationCommand, SaveEraWeaponModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-modifications/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraWeaponModification")]
    [ProducesResponseType(typeof(DeleteEraWeaponModificationResponse), 200)]
    public async Task<IActionResult> DeleteWeaponModificationAsync([FromRoute] Guid id, [FromBody] DeleteEraWeaponModificationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraWeaponModificationCommand, DeleteEraWeaponModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-templates/{id:guid}/disable")]
    [DescriptionLocalization("DisableEraWeaponTemplate")]
    [ProducesResponseType(typeof(DisableEraWeaponTemplateResponse), 200)]
    public async Task<IActionResult> DisableWeaponTemplateAsync([FromRoute] Guid id, [FromBody] DisableEraWeaponTemplateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DisableEraWeaponTemplateCommand, DisableEraWeaponTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/configuration/validate")]
    [DescriptionLocalization("ValidateEraWeaponConfiguration")]
    [ProducesResponseType(typeof(ValidateEraWeaponConfigurationResponse), 200)]
    public async Task<IActionResult> ValidateWeaponConfigurationAsync([FromBody] ValidateEraWeaponConfigurationRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<ValidateEraWeaponConfigurationRequest, ValidateEraWeaponConfigurationResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/configuration/publish")]
    [DescriptionLocalization("PublishEraWeaponConfiguration")]
    [ProducesResponseType(typeof(PublishEraWeaponConfigurationResponse), 200)]
    public async Task<IActionResult> PublishWeaponConfigurationAsync([FromBody] PublishEraWeaponConfigurationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<PublishEraWeaponConfigurationCommand, PublishEraWeaponConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
    [HttpGet("equipment/weapon-slot-types")]
    [DescriptionLocalization("GetEraWeaponSlotTypes")]
    [ProducesResponseType(typeof(GetEraWeaponSlotTypesResponse), 200)]
    public async Task<IActionResult> GetWeaponSlotTypesAsync([FromQuery] GetEraWeaponSlotTypesRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponSlotTypesRequest, GetEraWeaponSlotTypesResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-slot-types/{id:guid}")]
    [DescriptionLocalization("GetEraWeaponSlotType")]
    [ProducesResponseType(typeof(GetEraWeaponSlotTypeResponse), 200)]
    public async Task<IActionResult> GetWeaponSlotTypeAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponSlotTypeRequest, GetEraWeaponSlotTypeResponse>(new GetEraWeaponSlotTypeRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-types")]
    [DescriptionLocalization("SaveEraWeaponSlotType")]
    [ProducesResponseType(typeof(SaveEraWeaponSlotTypeResponse), 200)]
    public async Task<IActionResult> CreateWeaponSlotTypeAsync([FromBody] SaveEraWeaponSlotTypeCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraWeaponSlotTypeCommand, SaveEraWeaponSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-types/{id:guid}")]
    [DescriptionLocalization("SaveEraWeaponSlotType")]
    [ProducesResponseType(typeof(SaveEraWeaponSlotTypeResponse), 200)]
    public async Task<IActionResult> SaveWeaponSlotTypeAsync([FromRoute] Guid id, [FromBody] SaveEraWeaponSlotTypeCommand command, CancellationToken cancellationToken)
    {
        if (command.SlotType != null) command.SlotType.Id = id;
        var response = await _mediator.SendAsync<SaveEraWeaponSlotTypeCommand, SaveEraWeaponSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-types/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraWeaponSlotType")]
    [ProducesResponseType(typeof(DeleteEraWeaponSlotTypeResponse), 200)]
    public async Task<IActionResult> DeleteWeaponSlotTypeAsync([FromRoute] Guid id, [FromBody] DeleteEraWeaponSlotTypeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraWeaponSlotTypeCommand, DeleteEraWeaponSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-slot-configurations")]
    [DescriptionLocalization("GetEraWeaponSlotConfigurations")]
    [ProducesResponseType(typeof(GetEraWeaponSlotConfigurationsResponse), 200)]
    public async Task<IActionResult> GetWeaponSlotConfigurationsAsync([FromQuery] GetEraWeaponSlotConfigurationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponSlotConfigurationsRequest, GetEraWeaponSlotConfigurationsResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/weapon-slot-configurations/{id:guid}")]
    [DescriptionLocalization("GetEraWeaponSlotConfiguration")]
    [ProducesResponseType(typeof(GetEraWeaponSlotConfigurationResponse), 200)]
    public async Task<IActionResult> GetWeaponSlotConfigurationAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraWeaponSlotConfigurationRequest, GetEraWeaponSlotConfigurationResponse>(new GetEraWeaponSlotConfigurationRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-configurations")]
    [DescriptionLocalization("SaveEraWeaponSlotConfiguration")]
    [ProducesResponseType(typeof(SaveEraWeaponSlotConfigurationResponse), 200)]
    public async Task<IActionResult> CreateWeaponSlotConfigurationAsync([FromBody] SaveEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraWeaponSlotConfigurationCommand, SaveEraWeaponSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-configurations/{id:guid}")]
    [DescriptionLocalization("SaveEraWeaponSlotConfiguration")]
    [ProducesResponseType(typeof(SaveEraWeaponSlotConfigurationResponse), 200)]
    public async Task<IActionResult> SaveWeaponSlotConfigurationAsync([FromRoute] Guid id, [FromBody] SaveEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        if (command.SlotConfiguration != null) command.SlotConfiguration.Id = id;
        var response = await _mediator.SendAsync<SaveEraWeaponSlotConfigurationCommand, SaveEraWeaponSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/weapon-slot-configurations/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraWeaponSlotConfiguration")]
    [ProducesResponseType(typeof(DeleteEraWeaponSlotConfigurationResponse), 200)]
    public async Task<IActionResult> DeleteWeaponSlotConfigurationAsync([FromRoute] Guid id, [FromBody] DeleteEraWeaponSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraWeaponSlotConfigurationCommand, DeleteEraWeaponSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

}
