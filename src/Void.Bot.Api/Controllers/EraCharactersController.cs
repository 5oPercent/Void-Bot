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
[Route("api/era/characters")]
[RequestSizeLimit(16384)]
[OpenApiTagOrder(402)]
public sealed partial class EraCharactersController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraCharactersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraCharacterList")]
    [ProducesResponseType(typeof(GetEraCharacterListResponse), 200)]
    public async Task<IActionResult> GetCharacterListAsync(
        [FromQuery] GetEraCharacterListRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraCharacterListRequest, GetEraCharacterListResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{characterId:guid}")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraCharacter")]
    [ProducesResponseType(typeof(GetEraCharacterResponse), 200)]
    public async Task<IActionResult> GetCharacterAsync(
        [FromRoute] GetEraCharacterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraCharacterRequest, GetEraCharacterResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("")]
    [SkipPermissionCheck]
    [DescriptionLocalization("CreateEraCharacter")]
    [ProducesResponseType(typeof(CreateEraCharacterResponse), 200)]
    public async Task<IActionResult> CreateCharacterAsync(
        [FromBody] CreateEraCharacterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<CreateEraCharacterCommand, CreateEraCharacterResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{characterId:guid}/equipment")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraCharacterEquipment")]
    [ProducesResponseType(typeof(GetEraCharacterEquipmentResponse), 200)]
    public async Task<IActionResult> GetEquipmentAsync(
        [FromRoute] Guid characterId, [FromQuery] GetEraCharacterEquipmentRequest request,
        CancellationToken cancellationToken)
    {
        request.CharacterId = characterId;
        var response = await _mediator
            .RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("{characterId:guid}/inventory")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraCharacterInventory")]
    [ProducesResponseType(typeof(GetEraCharacterInventoryResponse), 200)]
    public async Task<IActionResult> GetInventoryAsync(
        [FromRoute] Guid characterId, [FromQuery] GetEraCharacterInventoryRequest request,
        CancellationToken cancellationToken)
    {
        request.CharacterId = characterId;
        var response = await _mediator
            .RequestAsync<GetEraCharacterInventoryRequest, GetEraCharacterInventoryResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{characterId:guid}/equipment/equip")]
    [SkipPermissionCheck]
    [DescriptionLocalization("EquipEraEquipment")]
    [ProducesResponseType(typeof(EquipEraEquipmentResponse), 200)]
    public async Task<IActionResult> EquipAsync(
        [FromRoute] Guid characterId, [FromBody] EquipEraEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<EquipEraEquipmentCommand, EquipEraEquipmentResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{characterId:guid}/equipment/unequip")]
    [SkipPermissionCheck]
    [DescriptionLocalization("UnequipEraEquipment")]
    [ProducesResponseType(typeof(UnequipEraEquipmentResponse), 200)]
    public async Task<IActionResult> UnequipAsync(
        [FromRoute] Guid characterId, [FromBody] UnequipEraEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        var response = await _mediator
            .SendAsync<UnequipEraEquipmentCommand, UnequipEraEquipmentResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("{characterId:guid}/inventory/{equipmentId:guid}/drop")]
    [SkipPermissionCheck]
    [DescriptionLocalization("DropEraInventoryEquipment")]
    [ProducesResponseType(typeof(DropEraInventoryEquipmentResponse), 200)]
    public async Task<IActionResult> DropInventoryEquipmentAsync(
        [FromRoute] Guid characterId, [FromRoute] Guid equipmentId, [FromBody] DropEraInventoryEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        command.EquipmentId = equipmentId;
        return Ok(await _mediator.SendAsync<DropEraInventoryEquipmentCommand, DropEraInventoryEquipmentResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{characterId:guid}/inventory/destroy")]
    [SkipPermissionCheck]
    [DescriptionLocalization("DestroyEraInventoryEquipment")]
    [ProducesResponseType(typeof(DestroyEraInventoryEquipmentResponse), 200)]
    public async Task<IActionResult> DestroyInventoryEquipmentAsync(
        [FromRoute] Guid characterId, [FromBody] DestroyEraInventoryEquipmentCommand command,
        CancellationToken cancellationToken)
    {
        command.CharacterId = characterId;
        return Ok(await _mediator.SendAsync<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>(command, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{characterId:guid}/delete")]
    [SkipPermissionCheck]
    [DescriptionLocalization("DeleteEraCharacter")]
    [ProducesResponseType(typeof(DeleteEraCharacterResponse), 200)]
    public async Task<IActionResult> DeleteCharacterAsync(
        [FromRoute] DeleteEraCharacterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<DeleteEraCharacterCommand, DeleteEraCharacterResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }
}
