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
public sealed class EraCharactersController : ControllerBase
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
