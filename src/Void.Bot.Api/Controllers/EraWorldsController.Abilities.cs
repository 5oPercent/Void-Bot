using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraWorldsController
{
    [HttpGet("{worldInstanceId:guid}/ability-profiles")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraWorldAbilityProfiles")]
    [ProducesResponseType(typeof(GetEraWorldAbilityProfilesResponse), 200)]
    public async Task<IActionResult> GetAbilityProfilesAsync([FromRoute] Guid worldInstanceId,
        [FromQuery] GetEraWorldAbilityProfilesRequest request, CancellationToken cancellationToken)
    {
        request.WorldInstanceId = worldInstanceId;
        var response = await _mediator.RequestAsync<GetEraWorldAbilityProfilesRequest, GetEraWorldAbilityProfilesResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
