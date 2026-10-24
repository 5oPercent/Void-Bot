using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraWorldsController
{
    [HttpGet("{worldInstanceId:guid}/equipment")]
    [SkipPermissionCheck]
    [DescriptionLocalization("GetEraWorldEquipment")]
    [ProducesResponseType(typeof(GetEraWorldEquipmentResponse), 200)]
    public async Task<IActionResult> GetEquipmentAsync([FromRoute] Guid worldInstanceId,
        [FromQuery] GetEraWorldEquipmentRequest request, CancellationToken cancellationToken)
    {
        request.WorldInstanceId = worldInstanceId;
        var response = await _mediator.RequestAsync<GetEraWorldEquipmentRequest, GetEraWorldEquipmentResponse>(request, cancellationToken).ConfigureAwait(false);
        return Ok(response);
    }
}
