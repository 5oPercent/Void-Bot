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
[Route("api/era")]
[RequestSizeLimit(16384)]
[OpenApiTagOrder(200)]
public class EraController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("config")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraConfig")]
    [ProducesResponseType(typeof(GetEraConfigResponse), 200)]
    public async Task<IActionResult> GetConfigAsync(
        [FromQuery] GetEraConfigRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraConfigRequest, GetEraConfigResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("me")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraPlayer")]
    [ProducesResponseType(typeof(GetEraPlayerResponse), 200)]
    public async Task<IActionResult> GetPlayerAsync(
        [FromQuery] GetEraPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraPlayerRequest, GetEraPlayerResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("me/initialize")]
    [SkipPermissionCheck]
    [DescriptionLocalization("InitializeEraPlayer")]
    [ProducesResponseType(typeof(InitializeEraPlayerResponse), 200)]
    public async Task<IActionResult> InitializePlayerAsync(
        [FromBody] InitializeEraPlayerCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<InitializeEraPlayerCommand, InitializeEraPlayerResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("me/characters")]
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

    [HttpGet("me/characters/{characterId:guid}")]
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

    [HttpPost("me/characters")]
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

    [HttpPost("me/characters/{characterId:guid}/delete")]
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
    [HttpGet("admin/players/{playerId:guid}")]
    [DescriptionLocalization("GetEraAdminPlayer")]
    [ProducesResponseType(typeof(GetEraAdminPlayerResponse), 200)]
    public async Task<IActionResult> GetAdminPlayerAsync(
        [FromRoute] GetEraAdminPlayerRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraAdminPlayerRequest, GetEraAdminPlayerResponse>(request, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("admin/config")]
    [DescriptionLocalization("UpdateEraConfig")]
    [ProducesResponseType(typeof(UpdateEraConfigResponse), 200)]
    public async Task<IActionResult> UpdateConfigAsync(
        [FromBody] UpdateEraConfigCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<UpdateEraConfigCommand, UpdateEraConfigResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("admin/players/{playerId:guid}/warehouse-capacity")]
    [DescriptionLocalization("SetEraWarehouseCapacity")]
    [ProducesResponseType(typeof(SetEraWarehouseCapacityResponse), 200)]
    public async Task<IActionResult> SetCapacityAsync(
        [FromRoute] Guid playerId,
        [FromBody] SetEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        command.PlayerId = playerId;
        var response = await _mediator
            .SendAsync<SetEraWarehouseCapacityCommand, SetEraWarehouseCapacityResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("admin/players/{playerId:guid}/warehouse-capacity-adjustments")]
    [DescriptionLocalization("AdjustEraWarehouseCapacity")]
    [ProducesResponseType(typeof(AdjustEraWarehouseCapacityResponse), 200)]
    public async Task<IActionResult> AdjustCapacityAsync(
        [FromRoute] Guid playerId,
        [FromBody] AdjustEraWarehouseCapacityCommand command,
        CancellationToken cancellationToken)
    {
        command.PlayerId = playerId;
        var response = await _mediator
            .SendAsync<AdjustEraWarehouseCapacityCommand, AdjustEraWarehouseCapacityResponse>(command, cancellationToken)
            .ConfigureAwait(false);
        return Ok(response);
    }
}
