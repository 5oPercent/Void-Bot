using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Steam;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Core.Handler.RequestHandler.Identity;

public class GenerateAuthenticateUrlFromSteamHandler : IRequestHandler<GenerateAuthenticateUrlFromSteamRequest, GenerateAuthenticateUrlFromSteamResponse>
{
    private readonly ISteamService _service;

    public GenerateAuthenticateUrlFromSteamHandler(ISteamService service)
    {
        _service = service;
    }
    
    public Task<GenerateAuthenticateUrlFromSteamResponse> Handle(IReceiveContext<GenerateAuthenticateUrlFromSteamRequest> context, CancellationToken cancellationToken)
    {
        return Task.FromResult(new GenerateAuthenticateUrlFromSteamResponse
        {
            Data = _service.GenerateAuthenticateUrl(context.Message.Authorization)
        });
    }
}