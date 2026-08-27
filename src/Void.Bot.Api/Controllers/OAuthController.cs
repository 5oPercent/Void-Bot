using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Authentication;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Enum;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(110)]
public sealed class OAuthController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly OAuthSetting _oauthSetting;

    public OAuthController(IMediator mediator, OAuthSetting oauthSetting)
    {
        _mediator = mediator;
        _oauthSetting = oauthSetting;
    }

    [AllowAnonymous]
    [HttpGet("{platform}/authorize")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(OAuthAuthorizeResponse), 200)]
    public async Task<IActionResult> AuthorizeAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthAuthorizeCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthAuthorizeCommand, OAuthAuthorizeResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet("{platform}/callback")]
    [ProducesResponseType(302)]
    public async Task<IActionResult> CallbackAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthCallbackCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthCallbackCommand, OAuthCallbackResponse>(command).ConfigureAwait(false);

        return Redirect(OAuthFrontendRedirectUrl.Create(_oauthSetting, platform, response));
    }

    [AllowAnonymous]
    [HttpPost("exchange")]
    [ProducesResponseType(typeof(OAuthExchangeResponse), 200)]
    public async Task<IActionResult> ExchangeAsync([FromBody] OAuthExchangeCommand command)
    {
        var response = await _mediator.SendAsync<OAuthExchangeCommand, OAuthExchangeResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(OAuthRegisterResponse), 200)]
    public async Task<IActionResult> RegisterAsync([FromBody] OAuthRegisterCommand command)
    {
        var response = await _mediator.SendAsync<OAuthRegisterCommand, OAuthRegisterResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("bind")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(OAuthBindResponse), 200)]
    public async Task<IActionResult> BindAsync([FromBody] OAuthBindCommand command)
    {
        var response = await _mediator.SendAsync<OAuthBindCommand, OAuthBindResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost("{platform}/unlink")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(OAuthUnlinkResponse), 200)]
    public async Task<IActionResult> UnlinkAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthUnlinkCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthUnlinkCommand, OAuthUnlinkResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }
}
