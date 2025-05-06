using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Request.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class Destiny2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public Destiny2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }
    
    [HttpGet]
    [Route("player/profile/base")]
    public async Task<IActionResult> GetPlayerBaseProfileByBungieName([FromQuery] GetDestiny2BaseProfileRequest request)
    {
        var response = await _mediator.RequestAsync<GetDestiny2BaseProfileRequest, GetDestiny2BaseProfileResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [HttpGet]
    [Route("player/profile/platform")]
    public async Task<IActionResult> GetPlayerProfileByBungieName([FromQuery] GetDestinyProfileListByBungieNameRequest request)
    {
        var response = await _mediator.RequestAsync<GetDestinyProfileListByBungieNameRequest, GetDestinyProfileListByBungieNameResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [HttpGet]
    [Route("player/career")]
    public async Task<IActionResult> GetPlayerCareer([FromQuery] GetDestiny2CareerRequest request)
    {
        var response = await _mediator.RequestAsync<GetDestiny2CareerRequest, GetDestiny2CareerResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
}