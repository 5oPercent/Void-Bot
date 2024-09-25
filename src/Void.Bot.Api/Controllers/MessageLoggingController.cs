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
    public async Task<IActionResult> GetMessageLogAsync([FromQuery] GetMessageLogPageRequest pageRequest)
    {
        var response = await mediator.RequestAsync<GetMessageLogPageRequest, GetMessageLogPageResponse>(pageRequest).ConfigureAwait(false);
        
        return Ok(response);
    }
    
    [HttpGet]
    [Route("get/page")]
    public async Task<IActionResult> GetMessageLogPageAsync([FromQuery] GetMessageLogPageRequest request)
    {
        var response = await mediator.RequestAsync<GetMessageLogPageRequest, GetMessageLogPageResponse>(request).ConfigureAwait(false);
        
        return Ok(response);
    }
}