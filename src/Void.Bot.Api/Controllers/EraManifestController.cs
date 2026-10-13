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
[Route("api/era/manifest")]
[RequestSizeLimit(1048576)]
[OpenApiTagOrder(400)]
public sealed class EraManifestController : ControllerBase
{
    private readonly IMediator _mediator;

    public EraManifestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("")]
    [RequestSizeLimit(16384)]
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

    [HttpGet("catalog")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraSkillConfiguration")]
    [ProducesResponseType(typeof(GetEraSkillConfigurationResponse), 200)]
    public async Task<IActionResult> GetConfigurationAsync(
        [FromQuery] GetEraSkillConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .RequestAsync<GetEraSkillConfigurationRequest, GetEraSkillConfigurationResponse>(request, cancellationToken)
            .ConfigureAwait(false);

        return Ok(response);
    }
}
