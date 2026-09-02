using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord.RolePanel;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public sealed class GetDiscordRolePanelConfigRequestHandler(IDiscordRolePanelService service)
    : IRequestHandler<GetDiscordRolePanelConfigRequest, GetDiscordRolePanelConfigResponse>
{
    public Task<GetDiscordRolePanelConfigResponse> Handle(
        IReceiveContext<GetDiscordRolePanelConfigRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetConfigAsync(context.Message, cancellationToken);
    }
}
