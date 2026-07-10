using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public class DeleteDiscordVoiceChannelConfigCommandHandler : ICommandHandler<DeleteDiscordVoiceChannelConfigCommand, DeleteDiscordVoiceChannelConfigResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public DeleteDiscordVoiceChannelConfigCommandHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<DeleteDiscordVoiceChannelConfigResponse> Handle(IReceiveContext<DeleteDiscordVoiceChannelConfigCommand> context, CancellationToken cancellationToken)
    {
        return await _service.DeleteConfigAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
