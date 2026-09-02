using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public sealed class GetDiscordGuildRoleListRequestHandler(IDiscordRolePanelService service)
    : IRequestHandler<GetDiscordGuildRoleListRequest, GetDiscordGuildRoleListResponse>
{
    public Task<GetDiscordGuildRoleListResponse> Handle(
        IReceiveContext<GetDiscordGuildRoleListRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetGuildRoleListAsync(context.Message, cancellationToken);
    }
}
