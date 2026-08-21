using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
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
    public async Task<IActionResult> GetPlayerProfileByBungieName([FromQuery] GetDestiny2ProfileListByBungieNameRequest request)
    {
        var response = await _mediator.RequestAsync<GetDestiny2ProfileListByBungieNameRequest, GetDestiny2ProfileListByBungieNameResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [HttpGet]
    [Route("player/career")]
    public async Task<IActionResult> GetPlayerCareer([FromQuery] GetDestiny2CareerRequest request)
    {
        var response = await _mediator.RequestAsync<GetDestiny2CareerRequest, GetDestiny2CareerResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }

    [HttpPost]
    [Route("player/season")]
    public async Task<IActionResult> GetPlayerSeason([FromBody] GetDestiny2SeasonCommand command)
    {
        var response = await _mediator.SendAsync<GetDestiny2SeasonCommand, GetDestiny2SeasonResponse>(command).ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("weapon/types")]
    public async Task<IActionResult> GetWeaponTypes([FromQuery] GetDestiny2WeaponTypesRequest request)
    {
        var response = await _mediator
            .RequestAsync<GetDestiny2WeaponTypesRequest, GetDestiny2WeaponTypesResponse>(request)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpGet]
    [Route("weapon/perks")]
    public async Task<IActionResult> GetWeaponPerks([FromQuery] GetDestiny2WeaponPerksRequest request)
    {
        var response = await _mediator
            .RequestAsync<GetDestiny2WeaponPerksRequest, GetDestiny2WeaponPerksResponse>(request)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("weapon/godroll/filter")]
    public async Task<IActionResult> FilterWeaponGodRoll([FromBody] FilterDestiny2WeaponGodRollRequest request)
    {
        var response = await _mediator
            .RequestAsync<FilterDestiny2WeaponGodRollRequest, FilterDestiny2WeaponGodRollResponse>(request)
            .ConfigureAwait(false);

        return Ok(response);
    }

    [HttpPost]
    [Route("weapon/cache/rebuild")]
    public async Task<IActionResult> RebuildWeaponCache([FromBody] RebuildDestiny2WeaponCacheCommand command)
    {
        var response = await _mediator
            .SendAsync<RebuildDestiny2WeaponCacheCommand, RebuildDestiny2WeaponCacheResponse>(command)
            .ConfigureAwait(false);

        return StatusCode((int)response.Code, response);
    }
}
