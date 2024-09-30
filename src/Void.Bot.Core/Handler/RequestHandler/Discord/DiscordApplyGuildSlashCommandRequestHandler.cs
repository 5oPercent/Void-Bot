using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Handler.RequestHandler.Discord;

public class DiscordApplyGuildSlashCommandRequestHandler : IRequestHandler<DiscordApplyGuildSlashCommandRequest, DiscordApplyGuildSlashCommandResponse>
{
    private readonly IDiscordService _service;

    public DiscordApplyGuildSlashCommandRequestHandler(IDiscordService service)
    {
        _service = service;
    }
    
    public async Task<DiscordApplyGuildSlashCommandResponse> Handle(IReceiveContext<DiscordApplyGuildSlashCommandRequest> context, CancellationToken cancellationToken)
    { 
        await _service.ApplyGuildSlashCommandAsync(context.Message, cancellationToken).ConfigureAwait(false);

        return new DiscordApplyGuildSlashCommandResponse();
    }
}