using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraAdminController
{
    [HttpGet("equipment/armor-templates")]
    [DescriptionLocalization("GetEraArmorTemplates")]
    [ProducesResponseType(typeof(GetEraArmorTemplatesResponse), 200)]
    public async Task<IActionResult> GetArmorTemplatesAsync([FromQuery] GetEraArmorTemplatesRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorTemplatesRequest, GetEraArmorTemplatesResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-templates/{id:guid}")]
    [DescriptionLocalization("GetEraArmorTemplate")]
    [ProducesResponseType(typeof(GetEraArmorTemplateResponse), 200)]
    public async Task<IActionResult> GetArmorTemplateAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorTemplateRequest, GetEraArmorTemplateResponse>(new GetEraArmorTemplateRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-templates")]
    [DescriptionLocalization("SaveEraArmorTemplate")]
    [ProducesResponseType(typeof(SaveEraArmorTemplateResponse), 200)]
    public async Task<IActionResult> CreateArmorTemplateAsync([FromBody] SaveEraArmorTemplateCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraArmorTemplateCommand, SaveEraArmorTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-templates/{id:guid}")]
    [DescriptionLocalization("SaveEraArmorTemplate")]
    [ProducesResponseType(typeof(SaveEraArmorTemplateResponse), 200)]
    public async Task<IActionResult> SaveArmorTemplateAsync([FromRoute] Guid id, [FromBody] SaveEraArmorTemplateCommand command, CancellationToken cancellationToken)
    {
        if (command.Template != null) command.Template.Id = id;
        var response = await _mediator.SendAsync<SaveEraArmorTemplateCommand, SaveEraArmorTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-templates/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraArmorTemplate")]
    [ProducesResponseType(typeof(DeleteEraArmorTemplateResponse), 200)]
    public async Task<IActionResult> DeleteArmorTemplateAsync([FromRoute] Guid id, [FromBody] DeleteEraArmorTemplateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraArmorTemplateCommand, DeleteEraArmorTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-modifications")]
    [DescriptionLocalization("GetEraArmorModifications")]
    [ProducesResponseType(typeof(GetEraArmorModificationsResponse), 200)]
    public async Task<IActionResult> GetArmorModificationsAsync([FromQuery] GetEraArmorModificationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorModificationsRequest, GetEraArmorModificationsResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-modifications/{id:guid}")]
    [DescriptionLocalization("GetEraArmorModification")]
    [ProducesResponseType(typeof(GetEraArmorModificationResponse), 200)]
    public async Task<IActionResult> GetArmorModificationAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorModificationRequest, GetEraArmorModificationResponse>(new GetEraArmorModificationRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-modifications")]
    [DescriptionLocalization("SaveEraArmorModification")]
    [ProducesResponseType(typeof(SaveEraArmorModificationResponse), 200)]
    public async Task<IActionResult> CreateArmorModificationAsync([FromBody] SaveEraArmorModificationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraArmorModificationCommand, SaveEraArmorModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-modifications/{id:guid}")]
    [DescriptionLocalization("SaveEraArmorModification")]
    [ProducesResponseType(typeof(SaveEraArmorModificationResponse), 200)]
    public async Task<IActionResult> SaveArmorModificationAsync([FromRoute] Guid id, [FromBody] SaveEraArmorModificationCommand command, CancellationToken cancellationToken)
    {
        if (command.Modification != null) command.Modification.Id = id;
        var response = await _mediator.SendAsync<SaveEraArmorModificationCommand, SaveEraArmorModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-modifications/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraArmorModification")]
    [ProducesResponseType(typeof(DeleteEraArmorModificationResponse), 200)]
    public async Task<IActionResult> DeleteArmorModificationAsync([FromRoute] Guid id, [FromBody] DeleteEraArmorModificationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraArmorModificationCommand, DeleteEraArmorModificationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-templates/{id:guid}/disable")]
    [DescriptionLocalization("DisableEraArmorTemplate")]
    [ProducesResponseType(typeof(DisableEraArmorTemplateResponse), 200)]
    public async Task<IActionResult> DisableArmorTemplateAsync([FromRoute] Guid id, [FromBody] DisableEraArmorTemplateCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DisableEraArmorTemplateCommand, DisableEraArmorTemplateResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-configuration/validate")]
    [DescriptionLocalization("ValidateEraArmorConfiguration")]
    [ProducesResponseType(typeof(ValidateEraArmorConfigurationResponse), 200)]
    public async Task<IActionResult> ValidateArmorConfigurationAsync([FromBody] ValidateEraArmorConfigurationRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<ValidateEraArmorConfigurationRequest, ValidateEraArmorConfigurationResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-configuration/publish")]
    [DescriptionLocalization("PublishEraArmorConfiguration")]
    [ProducesResponseType(typeof(PublishEraArmorConfigurationResponse), 200)]
    public async Task<IActionResult> PublishArmorConfigurationAsync([FromBody] PublishEraArmorConfigurationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<PublishEraArmorConfigurationCommand, PublishEraArmorConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
    [HttpGet("equipment/armor-slot-types")]
    [DescriptionLocalization("GetEraArmorSlotTypes")]
    [ProducesResponseType(typeof(GetEraArmorSlotTypesResponse), 200)]
    public async Task<IActionResult> GetArmorSlotTypesAsync([FromQuery] GetEraArmorSlotTypesRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorSlotTypesRequest, GetEraArmorSlotTypesResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-slot-types/{id:guid}")]
    [DescriptionLocalization("GetEraArmorSlotType")]
    [ProducesResponseType(typeof(GetEraArmorSlotTypeResponse), 200)]
    public async Task<IActionResult> GetArmorSlotTypeAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorSlotTypeRequest, GetEraArmorSlotTypeResponse>(new GetEraArmorSlotTypeRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-types")]
    [DescriptionLocalization("SaveEraArmorSlotType")]
    [ProducesResponseType(typeof(SaveEraArmorSlotTypeResponse), 200)]
    public async Task<IActionResult> CreateArmorSlotTypeAsync([FromBody] SaveEraArmorSlotTypeCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraArmorSlotTypeCommand, SaveEraArmorSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-types/{id:guid}")]
    [DescriptionLocalization("SaveEraArmorSlotType")]
    [ProducesResponseType(typeof(SaveEraArmorSlotTypeResponse), 200)]
    public async Task<IActionResult> SaveArmorSlotTypeAsync([FromRoute] Guid id, [FromBody] SaveEraArmorSlotTypeCommand command, CancellationToken cancellationToken)
    {
        if (command.SlotType != null) command.SlotType.Id = id;
        var response = await _mediator.SendAsync<SaveEraArmorSlotTypeCommand, SaveEraArmorSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-types/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraArmorSlotType")]
    [ProducesResponseType(typeof(DeleteEraArmorSlotTypeResponse), 200)]
    public async Task<IActionResult> DeleteArmorSlotTypeAsync([FromRoute] Guid id, [FromBody] DeleteEraArmorSlotTypeCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraArmorSlotTypeCommand, DeleteEraArmorSlotTypeResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-slot-configurations")]
    [DescriptionLocalization("GetEraArmorSlotConfigurations")]
    [ProducesResponseType(typeof(GetEraArmorSlotConfigurationsResponse), 200)]
    public async Task<IActionResult> GetArmorSlotConfigurationsAsync([FromQuery] GetEraArmorSlotConfigurationsRequest request, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorSlotConfigurationsRequest, GetEraArmorSlotConfigurationsResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("equipment/armor-slot-configurations/{id:guid}")]
    [DescriptionLocalization("GetEraArmorSlotConfiguration")]
    [ProducesResponseType(typeof(GetEraArmorSlotConfigurationResponse), 200)]
    public async Task<IActionResult> GetArmorSlotConfigurationAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetEraArmorSlotConfigurationRequest, GetEraArmorSlotConfigurationResponse>(new GetEraArmorSlotConfigurationRequest { Id = id }, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-configurations")]
    [DescriptionLocalization("SaveEraArmorSlotConfiguration")]
    [ProducesResponseType(typeof(SaveEraArmorSlotConfigurationResponse), 200)]
    public async Task<IActionResult> CreateArmorSlotConfigurationAsync([FromBody] SaveEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SaveEraArmorSlotConfigurationCommand, SaveEraArmorSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-configurations/{id:guid}")]
    [DescriptionLocalization("SaveEraArmorSlotConfiguration")]
    [ProducesResponseType(typeof(SaveEraArmorSlotConfigurationResponse), 200)]
    public async Task<IActionResult> SaveArmorSlotConfigurationAsync([FromRoute] Guid id, [FromBody] SaveEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        if (command.SlotConfiguration != null) command.SlotConfiguration.Id = id;
        var response = await _mediator.SendAsync<SaveEraArmorSlotConfigurationCommand, SaveEraArmorSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("equipment/armor-slot-configurations/{id:guid}/delete")]
    [DescriptionLocalization("DeleteEraArmorSlotConfiguration")]
    [ProducesResponseType(typeof(DeleteEraArmorSlotConfigurationResponse), 200)]
    public async Task<IActionResult> DeleteArmorSlotConfigurationAsync([FromRoute] Guid id, [FromBody] DeleteEraArmorSlotConfigurationCommand command, CancellationToken cancellationToken)
    {
        command.Id = id;
        var response = await _mediator.SendAsync<DeleteEraArmorSlotConfigurationCommand, DeleteEraArmorSlotConfigurationResponse>(command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

}
