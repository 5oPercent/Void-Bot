using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
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
    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromQuery]AuthenticateRequest command)
    {
        var response = await _mediator.RequestAsync<AuthenticateRequest, AuthenticateResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("oauth/{platform}/authorize")]
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

        return Ok(response);
    }

    [HttpPost]
    [Route("oauth/{platform}/unlink")]
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
    
    [HttpGet]
    [Route("generate/url/bungie")]
    public async Task<IActionResult> GenerateAuthenticateUrlFromBungie([FromQuery]GenerateAuthenticateUrlFromBungieRequest request)
    {
        var response = await _mediator.RequestAsync<GenerateAuthenticateUrlFromBungieRequest, GenerateAuthenticateUrlFromBungieResponse>(request).ConfigureAwait(false);

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
    
    [AllowAnonymous]
    [HttpGet]
    [Route("link/bungie")]
    public async Task<IActionResult> LoginBungieAsync([FromQuery]LoginBungieCommand command)
    {
        var response = await _mediator.SendAsync<LoginBungieCommand, LoginBungieResponse>(command).ConfigureAwait(false);

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
