using Mediator.Net;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessageLoggingController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [Route("get")]
    public async Task<IActionResult> GetMessageLogAsync([FromQuery] GetMessageLogRequest request)
    {
        var response = await mediator.RequestAsync<GetMessageLogRequest, GetMessageLogResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
}