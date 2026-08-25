using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Authentication;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.OAuth;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(100)]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUser _currentUser;
    private readonly OAuthSetting _oauthSetting;

    public IdentityController(IMediator mediator, ICurrentUser currentUser, OAuthSetting oauthSetting)
    {
        _mediator = mediator;
        _currentUser = currentUser;
        _oauthSetting = oauthSetting;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> RegisterUserAccount([FromBody]RegisterUserAccountCommand command)
    {
        var response = await _mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register/email/send-code")]
    public async Task<IActionResult> SendRegistrationEmailCodeAsync(
        [FromBody] SendRegistrationEmailCodeCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<SendRegistrationEmailCodeCommand, SendRegistrationEmailCodeResponse>(
                command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpGet]
    [Route("email/status")]
    [SkipPermissionCheck]
    public async Task<IActionResult> GetEmailStatusAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetUserEmailStatusRequest, GetUserEmailStatusResponse>(
            new GetUserEmailStatusRequest(), cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/send-code")]
    [SkipPermissionCheck]
    public async Task<IActionResult> SendEmailBindingCodeAsync(
        [FromBody] SendEmailBindingCodeCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SendEmailBindingCodeCommand, SendEmailBindingCodeResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/bind")]
    [SkipPermissionCheck]
    public async Task<IActionResult> BindEmailAsync(
        [FromBody] BindUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<BindUserEmailCommand, BindUserEmailResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/unbind")]
    [SkipPermissionCheck]
    public async Task<IActionResult> UnbindEmailAsync(
        [FromBody] UnbindUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<UnbindUserEmailCommand, UnbindUserEmailResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("password-reset/send-code")]
    public async Task<IActionResult> SendPasswordResetCodeAsync(
        [FromBody] SendPasswordResetCodeCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<SendPasswordResetCodeCommand, SendPasswordResetCodeResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("password-reset")]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetUserAccountPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<ResetUserAccountPasswordCommand, ResetUserAccountPasswordResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromQuery]AuthenticateRequest command)
    {
        var response = await _mediator.RequestAsync<AuthenticateRequest, AuthenticateResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("refresh-token")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        var response = await _mediator.RequestAsync<RefreshTokenRequest, RefreshTokenResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("me")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(GetUserAccountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserAccountAsync()
    {
        var response = await _mediator.RequestAsync<GetUserAccountRequest, GetUserAccountResponse>(
            new GetUserAccountRequest { UserId = _currentUser.Id }).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("change-password")]
    [SkipPermissionCheck]
    public async Task<IActionResult> ChangePasswordAsync([FromBody] ChangeUserAccountPasswordCommand command)
    {
        command.UserId = _currentUser.Id;

        var response = await _mediator
            .SendAsync<ChangeUserAccountPasswordCommand, ChangeUserAccountPasswordResponse>(command)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("change-name/availability")]
    [SkipPermissionCheck]
    public async Task<IActionResult> CanChangeNameAsync([FromQuery] CanChangeUserAccountNameRequest request)
    {
        request.UserId = _currentUser.Id;

        var response = await _mediator
            .RequestAsync<CanChangeUserAccountNameRequest, CanChangeUserAccountNameResponse>(request)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("change-name")]
    [SkipPermissionCheck]
    public async Task<IActionResult> ChangeNameAsync([FromBody] ChangeUserAccountNameCommand command)
    {
        command.UserId = _currentUser.Id;

        var response = await _mediator
            .SendAsync<ChangeUserAccountNameCommand, ChangeUserAccountNameResponse>(command)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("oauth/{platform}/authorize")]
    [SkipPermissionCheck]
    public async Task<IActionResult> OAuthAuthorizeAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthAuthorizeCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthAuthorizeCommand, OAuthAuthorizeResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("oauth/{platform}/callback")]
    public async Task<IActionResult> OAuthCallbackAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthCallbackCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthCallbackCommand, OAuthCallbackResponse>(command).ConfigureAwait(false);

        return Redirect(OAuthFrontendRedirectUrl.Create(_oauthSetting, platform, response));
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("oauth/exchange")]
    public async Task<IActionResult> OAuthExchangeAsync([FromBody] OAuthExchangeCommand command)
    {
        var response = await _mediator.SendAsync<OAuthExchangeCommand, OAuthExchangeResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("oauth/register")]
    [SkipPermissionCheck]
    public async Task<IActionResult> OAuthRegisterAsync([FromBody] OAuthRegisterCommand command)
    {
        var response = await _mediator.SendAsync<OAuthRegisterCommand, OAuthRegisterResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("oauth/bind")]
    [SkipPermissionCheck]
    public async Task<IActionResult> OAuthBindAsync([FromBody] OAuthBindCommand command)
    {
        var response = await _mediator.SendAsync<OAuthBindCommand, OAuthBindResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("oauth/{platform}/unlink")]
    [SkipPermissionCheck]
    public async Task<IActionResult> OAuthUnlinkAsync(
        [FromRoute] AppPlatformEnum platform, [FromQuery] OAuthUnlinkCommand command)
    {
        command.Platform = platform;

        var response = await _mediator.SendAsync<OAuthUnlinkCommand, OAuthUnlinkResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("generate/url/steam")]
    public async Task<IActionResult> GenerateAuthenticateUrlFromSteam([FromQuery]GenerateAuthenticateUrlFromSteamRequest request)
    {
        var response = await _mediator.RequestAsync<GenerateAuthenticateUrlFromSteamRequest, GenerateAuthenticateUrlFromSteamResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [AllowAnonymous]
    [HttpGet]
    [Route("link/steam/{platform}/{accountId}")]
    public async Task<IActionResult> LoginSteamAsync(AppPlatformEnum platform, string accountId, [FromQuery]LoginSteamOIDCCommand command)
    {
        var request = new LoginSteamCommand
        {
            Authorization = new AuthorizationDto { Platform = platform, AccountId = accountId},
            OpenId = command.OpenId
        };
        
        var response = await _mediator.SendAsync<LoginSteamCommand, LoginSteamResponse>(request).ConfigureAwait(false);
            
        return Ok(response);
    }
    
    [HttpPost]
    [Route("apikey/cretae")]
    public async Task<IActionResult> CreateApiKeyAsync([FromBody]CreateApiKeyCommand request)
    {
        var response = await _mediator.SendAsync<CreateApiKeyCommand, CreateApiKeyResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
    
    [HttpPost]
    [Route("apikey/delete")]
    public async Task<IActionResult> DeleteApiKeyAsync([FromBody]DeleteApiKeyCommand request)
    {
        var response = await _mediator.SendAsync<DeleteApiKeyCommand, DeleteApiKeyResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }
}
