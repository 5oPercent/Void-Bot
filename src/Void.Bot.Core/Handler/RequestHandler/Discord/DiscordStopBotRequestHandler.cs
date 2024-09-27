using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class DiscordStopBotRequestHandler : IRequestHandler<DiscordStopBotRequest, DiscordStopBotResponse>
{
    private readonly IDiscordService _service;

    public DiscordStopBotRequestHandler(IDiscordService service)
    {
        _service = service;
    }
    
    public async Task<DiscordStopBotResponse> Handle(IReceiveContext<DiscordStopBotRequest> context, CancellationToken cancellationToken)
    {
        return await _service.StopAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}