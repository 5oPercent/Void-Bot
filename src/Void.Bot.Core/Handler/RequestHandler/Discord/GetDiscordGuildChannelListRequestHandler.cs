using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public sealed class GetDiscordGuildChannelListRequestHandler(IDiscordService service)
    : IRequestHandler<GetDiscordGuildChannelListRequest, GetDiscordGuildChannelListResponse>
{
    public Task<GetDiscordGuildChannelListResponse> Handle(
        IReceiveContext<GetDiscordGuildChannelListRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetGuildChannelListAsync(context.Message, cancellationToken);
    }
}
