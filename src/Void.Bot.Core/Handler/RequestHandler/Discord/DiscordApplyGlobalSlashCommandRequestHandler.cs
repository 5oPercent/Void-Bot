using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class DiscordApplySlashCommandRequestHandler : IRequestHandler<DiscordApplyGlobalSlashCommandRequest, DiscordApplyGlobalSlashCommandResponse>
{
    private readonly IDiscordService _service;

    public DiscordApplySlashCommandRequestHandler(IDiscordService service)
    {
        _service = service;
    }
    
    public async Task<DiscordApplyGlobalSlashCommandResponse> Handle(IReceiveContext<DiscordApplyGlobalSlashCommandRequest> context, CancellationToken cancellationToken)
    { 
        await _service.ApplyGlobalSlashCommandAsync(context.Message, cancellationToken).ConfigureAwait(false);

        return new DiscordApplyGlobalSlashCommandResponse();
    }
}