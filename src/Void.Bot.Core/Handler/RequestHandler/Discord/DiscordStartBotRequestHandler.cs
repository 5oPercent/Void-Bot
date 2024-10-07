using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class DiscordStartBotRequestHandler : IRequestHandler<DiscordStartBotRequest, DiscordStartBotResponse>
{
    private readonly IDiscordService _service;

    public DiscordStartBotRequestHandler(IDiscordService service)
    {
        _service = service;
    }
    
    public async Task<DiscordStartBotResponse> Handle(IReceiveContext<DiscordStartBotRequest> context, CancellationToken cancellationToken)
    {
        return await _service.StartAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}