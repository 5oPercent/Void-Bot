using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class GenerateAuthenticateUrlFromBungieHandler : IRequestHandler<GenerateAuthenticateUrlFromBungieRequest, GenerateAuthenticateUrlFromBungieResponse>
{
    private readonly IBungieService _service;

    public GenerateAuthenticateUrlFromBungieHandler(IBungieService service)
    {
        _service = service;
    }
    
    public async Task<GenerateAuthenticateUrlFromBungieResponse> Handle(IReceiveContext<GenerateAuthenticateUrlFromBungieRequest> context, CancellationToken cancellationToken)
    {
        return new GenerateAuthenticateUrlFromBungieResponse
        {
            Data = _service.GenerateAuthenticateUrl(context.Message.Authorization)
        };
    }
}