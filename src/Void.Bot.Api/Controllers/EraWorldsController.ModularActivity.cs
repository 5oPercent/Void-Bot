using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Api.Controllers;

public sealed partial class EraWorldsController
{
    [HttpPost("{worldInstanceId:guid}/modular-activity/approve")]
    [RequestSizeLimit(70000)]
    [SkipPermissionCheck] // 服务层显式要求有效 SuperAdmin；普通端点授权不能替代该角色。
    [DescriptionLocalization("ApproveEraModularActivity")]
    [ProducesResponseType(typeof(ApproveEraModularActivityResponse), 200)]
    public async Task<IActionResult> ApproveModularActivityAsync([FromRoute] Guid worldInstanceId,
        [FromBody] ApproveEraModularActivity request, CancellationToken cancellationToken)
    {
        request.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<ApproveEraModularActivity, ApproveEraModularActivityResponse>(request, cancellationToken).ConfigureAwait(false));
    }

    [HttpPost("{worldInstanceId:guid}/modular-activity/claim")]
    [SkipPermissionCheck] // 服务层核验当前权威房主及审批凭据。
    [DescriptionLocalization("ClaimEraModularActivity")]
    [ProducesResponseType(typeof(ClaimEraModularActivityResponse), 200)]
    public async Task<IActionResult> ClaimModularActivityAsync([FromRoute] Guid worldInstanceId,
        [FromBody] ClaimEraModularActivity request, CancellationToken cancellationToken)
    {
        request.WorldInstanceId = worldInstanceId;
        return Ok(await _mediator.SendAsync<ClaimEraModularActivity, ClaimEraModularActivityResponse>(request, cancellationToken).ConfigureAwait(false));
    }
}
