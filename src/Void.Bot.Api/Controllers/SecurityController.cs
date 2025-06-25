using Mediator.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Attributes;

namespace Void.Bot.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SecurityController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEndpointService _endpointService;

    public SecurityController(IMediator mediator, IEndpointService endpointService)
    {
        _mediator = mediator;
        _endpointService = endpointService;
    }
    
    [SkipScan]
    [AllowAnonymous]
    [HttpPost("endpoints")]
    public Task<IActionResult> SyncApiEndpoints()
    {
        var endpoints = _endpointService.GetEndpoints();
        
        return Task.FromResult<IActionResult>(Ok(endpoints));
    }
}