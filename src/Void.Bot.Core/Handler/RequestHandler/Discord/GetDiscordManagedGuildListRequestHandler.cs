using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public sealed class GetDiscordManagedGuildListRequestHandler(IDiscordService service)
    : IRequestHandler<GetDiscordManagedGuildListRequest, GetDiscordManagedGuildListResponse>
{
    public Task<GetDiscordManagedGuildListResponse> Handle(
        IReceiveContext<GetDiscordManagedGuildListRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetManagedGuildListAsync(context.Message, cancellationToken);
    }
}
