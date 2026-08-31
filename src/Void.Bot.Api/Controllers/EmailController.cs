using Void.Bot.Message.Attributes;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Commands.Email;
using Void.Bot.Message.Request.Email;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(210)]
public sealed class EmailController(IMediator mediator) : ControllerBase
{
    [HttpGet("oauth/authorize")]
    [ProducesResponseType(typeof(AuthorizeEmailResponse), 200)]
    [DescriptionLocalization("Authorize")]
    public async Task<IActionResult> AuthorizeAsync(CancellationToken cancellationToken)
    {
        var response = await mediator.SendAsync<AuthorizeEmailCommand, AuthorizeEmailResponse>(
            new AuthorizeEmailCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("oauth/callback")]
    [ProducesResponseType(typeof(CompleteEmailAuthorizationResponse), 200)]
    [DescriptionLocalization("Callback")]
    public async Task<IActionResult> CallbackAsync(
        [FromQuery] CompleteEmailAuthorizationCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.SendAsync<CompleteEmailAuthorizationCommand, CompleteEmailAuthorizationResponse>(
            command,
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("oauth/reauthorize")]
    [ProducesResponseType(typeof(AuthorizeEmailResponse), 200)]
    [DescriptionLocalization("Reauthorize")]
    public async Task<IActionResult> ReauthorizeAsync(CancellationToken cancellationToken)
    {
        var response = await mediator.SendAsync<AuthorizeEmailCommand, AuthorizeEmailResponse>(
            new AuthorizeEmailCommand { ForceReauthorization = true },
            cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(GetEmailServiceStatusResponse), 200)]
    [DescriptionLocalization("GetStatus")]
    public async Task<IActionResult> GetStatusAsync(CancellationToken cancellationToken)
    {
        var response = await mediator.RequestAsync<GetEmailServiceStatusRequest, GetEmailServiceStatusResponse>(
            new GetEmailServiceStatusRequest(), cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("test")]
    [ProducesResponseType(typeof(SendTestEmailResponse), 200)]
    [DescriptionLocalization("SendTestEmail")]
    public async Task<IActionResult> SendTestEmailAsync(
        [FromBody] SendTestEmailCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator.SendAsync<SendTestEmailCommand, SendTestEmailResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
