using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Dto.Authorization;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IdentityController : ControllerBase
{
    private IMediator _mediator;

    public IdentityController(IMediator mediator)
    {
        _mediator = mediator;
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
    public async Task<IActionResult> LoginSteamAsync(AppPlatformEnum platform, string accountId, [FromQuery]LoginSteamOIDCRequest oidcRequest)
    {
        try
        {
            var request = new LoginSteamRequest
            {
                Authorization = new AuthorizationDto { Platform = platform, AccountId = accountId},
                OpenID = oidcRequest.OpenID
            };
        
            var response = await _mediator.RequestAsync<LoginSteamRequest, LoginSteamResponse>(request).ConfigureAwait(false);
            
            return Ok(response);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }
    
    [HttpGet]
    [Route("link/bungie")]
    public async Task<IActionResult> LoginBungieAsync([FromQuery]LoginBungieRequest request)
    {
        try
        {
            var response = await _mediator.RequestAsync<LoginBungieRequest, LoginBungieResponse>(request).ConfigureAwait(false);

            return Ok(response);
        }
        catch (Exception ex)
        {
            return Ok(ex.Message);
        }
    }
}