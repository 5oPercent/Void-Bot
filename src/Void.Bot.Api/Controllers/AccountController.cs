using Void.Bot.Message.Attributes;
using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(190)]
public sealed class AccountController(IMediator mediator) : ControllerBase
{
    [HttpPost("password/reset-link")]
    [ProducesResponseType(typeof(SendUserAccountPasswordResetLinkResponse), 200)]
    [DescriptionLocalization("SendUserAccountPasswordResetLink")]
    public async Task<IActionResult> SendUserAccountPasswordResetLinkAsync(
        [FromBody] SendUserAccountPasswordResetLinkCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator
            .SendAsync<SendUserAccountPasswordResetLinkCommand, SendUserAccountPasswordResetLinkResponse>(
                command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("password/reset")]
    [ProducesResponseType(typeof(ForceResetUserAccountPasswordResponse), 200)]
    [DescriptionLocalization("ForceResetPassword")]
    public async Task<IActionResult> ForceResetPasswordAsync(
        [FromBody] ForceResetUserAccountPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator
            .SendAsync<ForceResetUserAccountPasswordCommand, ForceResetUserAccountPasswordResponse>(
                command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost("oauth/force-unlink")]
    [ProducesResponseType(typeof(ForceUnlinkUserAccountThirdPartyResponse), 200)]
    [DescriptionLocalization("ForceUnlinkThirdParty")]
    public async Task<IActionResult> ForceUnlinkThirdPartyAsync(
        [FromBody] ForceUnlinkUserAccountThirdPartyCommand command,
        CancellationToken cancellationToken)
    {
        var response = await mediator
            .SendAsync<ForceUnlinkUserAccountThirdPartyCommand, ForceUnlinkUserAccountThirdPartyResponse>(
                command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
