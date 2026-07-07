using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class GetDiscordVoiceChannelConfigRequestHandler : IRequestHandler<GetDiscordVoiceChannelConfigRequest, GetDiscordVoiceChannelConfigResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public GetDiscordVoiceChannelConfigRequestHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<GetDiscordVoiceChannelConfigResponse> Handle(IReceiveContext<GetDiscordVoiceChannelConfigRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetConfigAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
