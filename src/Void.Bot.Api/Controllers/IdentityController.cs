using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Core.Services.Identity;
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

    public IdentityController(IMediator mediator, ICurrentUser currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register")]
    [ProducesResponseType(typeof(RegisterUserAccountResponse), 200)]
    [DescriptionLocalization("RegisterUserAccount")]
    public async Task<IActionResult> RegisterUserAccount([FromBody]RegisterUserAccountCommand command)
    {
        var response = await _mediator.SendAsync<RegisterUserAccountCommand, RegisterUserAccountResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("register/email/send-code")]
    [ProducesResponseType(typeof(SendRegistrationEmailCodeResponse), 200)]
    [DescriptionLocalization("SendRegistrationEmailCode")]
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
    [ProducesResponseType(typeof(GetUserEmailStatusResponse), 200)]
    [DescriptionLocalization("GetEmailStatus")]
    public async Task<IActionResult> GetEmailStatusAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator.RequestAsync<GetUserEmailStatusRequest, GetUserEmailStatusResponse>(
            new GetUserEmailStatusRequest(), cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/send-code")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(SendEmailBindingCodeResponse), 200)]
    [DescriptionLocalization("SendEmailBindingCode")]
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
    [ProducesResponseType(typeof(BindUserEmailResponse), 200)]
    [DescriptionLocalization("BindEmail")]
    public async Task<IActionResult> BindEmailAsync(
        [FromBody] BindUserEmailCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<BindUserEmailCommand, BindUserEmailResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/unbind/send-code")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(SendEmailUnbindingCodeResponse), 200)]
    [DescriptionLocalization("SendEmailUnbindingCode")]
    public async Task<IActionResult> SendEmailUnbindingCodeAsync(CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<SendEmailUnbindingCodeCommand, SendEmailUnbindingCodeResponse>(
                new SendEmailUnbindingCodeCommand(), cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [HttpPost]
    [Route("email/unbind")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(UnbindUserEmailResponse), 200)]
    [DescriptionLocalization("UnbindEmail")]
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
    [ProducesResponseType(typeof(SendPasswordResetCodeResponse), 200)]
    [DescriptionLocalization("SendPasswordResetCode")]
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
    [ProducesResponseType(typeof(ResetUserAccountPasswordResponse), 200)]
    [DescriptionLocalization("ResetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetUserAccountPasswordCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator.SendAsync<ResetUserAccountPasswordCommand, ResetUserAccountPasswordResponse>(
            command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("password-reset/link")]
    [ProducesResponseType(typeof(ResetUserAccountPasswordByLinkResponse), 200)]
    [DescriptionLocalization("ResetUserAccountPasswordByLink")]
    public async Task<IActionResult> ResetUserAccountPasswordByLinkAsync(
        [FromBody] ResetUserAccountPasswordByLinkCommand command,
        CancellationToken cancellationToken)
    {
        var response = await _mediator
            .SendAsync<ResetUserAccountPasswordByLinkCommand, ResetUserAccountPasswordByLinkResponse>(
                command, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("login")]
    [ProducesResponseType(typeof(AuthenticateResponse), 200)]
    [DescriptionLocalization("Login")]
    public async Task<IActionResult> LoginAsync([FromQuery]AuthenticateRequest command)
    {
        var response = await _mediator.RequestAsync<AuthenticateRequest, AuthenticateResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost]
    [Route("refresh-token")]
    [ProducesResponseType(typeof(RefreshTokenResponse), 200)]
    [DescriptionLocalization("RefreshToken")]
    public async Task<IActionResult> RefreshTokenAsync([FromBody] RefreshTokenRequest request)
    {
        var response = await _mediator.RequestAsync<RefreshTokenRequest, RefreshTokenResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("me")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(GetUserAccountResponse), StatusCodes.Status200OK)]
    [DescriptionLocalization("GetCurrentUserAccount")]
    public async Task<IActionResult> GetCurrentUserAccountAsync()
    {
        var response = await _mediator.RequestAsync<GetUserAccountRequest, GetUserAccountResponse>(
            new GetUserAccountRequest { UserId = _currentUser.Id }).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("change-password")]
    [SkipPermissionCheck]
    [ProducesResponseType(typeof(ChangeUserAccountPasswordResponse), 200)]
    [DescriptionLocalization("ChangePassword")]
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
    [ProducesResponseType(typeof(CanChangeUserAccountNameResponse), 200)]
    [DescriptionLocalization("CanChangeName")]
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
    [ProducesResponseType(typeof(ChangeUserAccountNameResponse), 200)]
    [DescriptionLocalization("ChangeName")]
    public async Task<IActionResult> ChangeNameAsync([FromBody] ChangeUserAccountNameCommand command)
    {
        command.UserId = _currentUser.Id;

        var response = await _mediator
            .SendAsync<ChangeUserAccountNameCommand, ChangeUserAccountNameResponse>(command)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("generate/url/steam")]
    [ProducesResponseType(typeof(GenerateAuthenticateUrlFromSteamResponse), 200)]
    [DescriptionLocalization("GenerateAuthenticateUrlFromSteam")]
    public async Task<IActionResult> GenerateAuthenticateUrlFromSteam([FromQuery]GenerateAuthenticateUrlFromSteamRequest request)
    {
        var response = await _mediator.RequestAsync<GenerateAuthenticateUrlFromSteamRequest, GenerateAuthenticateUrlFromSteamResponse>(request).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("link/steam/{platform}/{accountId}")]
    [ProducesResponseType(typeof(LoginSteamResponse), 200)]
    [DescriptionLocalization("LoginSteam")]
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
}
