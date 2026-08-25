using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Api.Filters;
using Void.Bot.Api.OpenApi;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Security]
[OpenApiTagOrder(230)]
public class MessageLoggingController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("page")]
    public async Task<IActionResult> GetMessageLogPageAsync([FromQuery] GetMessageLogPageRequest request)
    {
        var response = await mediator.RequestAsync<GetMessageLogPageRequest, GetMessageLogPageResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
}
