using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class GetDiscordTemporaryVoiceChannelListRequestHandler : IRequestHandler<GetDiscordTemporaryVoiceChannelListRequest, GetDiscordTemporaryVoiceChannelListResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public GetDiscordTemporaryVoiceChannelListRequestHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<GetDiscordTemporaryVoiceChannelListResponse> Handle(IReceiveContext<GetDiscordTemporaryVoiceChannelListRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetTemporaryChannelListAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
