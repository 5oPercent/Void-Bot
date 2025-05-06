using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Commands.Identity;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private readonly IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [AllowAnonymous]
    [HttpGet]
    [Route("login")]
    public async Task<IActionResult> LoginAsync([FromQuery]AuthenticateRequest command)
    {
        var response = await _mediator.RequestAsync<AuthenticateRequest, AuthenticateResponse>(command).ConfigureAwait(false);

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