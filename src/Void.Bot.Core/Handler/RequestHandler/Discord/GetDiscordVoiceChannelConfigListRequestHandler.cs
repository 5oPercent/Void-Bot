using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class GetDiscordVoiceChannelConfigListRequestHandler : IRequestHandler<GetDiscordVoiceChannelConfigListRequest, GetDiscordVoiceChannelConfigListResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public GetDiscordVoiceChannelConfigListRequestHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<GetDiscordVoiceChannelConfigListResponse> Handle(IReceiveContext<GetDiscordVoiceChannelConfigListRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetConfigListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
