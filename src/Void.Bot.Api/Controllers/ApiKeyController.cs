using Void.Bot.Message.Attributes;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Commands.Identity;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(120)]
public sealed class ApiKeyController(IMediator mediator) : ControllerBase
{
    [HttpPost("create")]
    [ProducesResponseType(typeof(CreateApiKeyResponse), 200)]
    [DescriptionLocalization("Create")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateApiKeyCommand command)
    {
        var response = await mediator.SendAsync<CreateApiKeyCommand, CreateApiKeyResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost("delete")]
    [ProducesResponseType(typeof(DeleteApiKeyResponse), 200)]
    [DescriptionLocalization("Delete")]
    public async Task<IActionResult> DeleteAsync([FromBody] DeleteApiKeyCommand command)
    {
        var response = await mediator.SendAsync<DeleteApiKeyCommand, DeleteApiKeyResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
}
