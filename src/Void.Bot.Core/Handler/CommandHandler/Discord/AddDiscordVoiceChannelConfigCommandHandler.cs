using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public class AddDiscordVoiceChannelConfigCommandHandler : ICommandHandler<AddDiscordVoiceChannelConfigCommand, AddDiscordVoiceChannelConfigResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public AddDiscordVoiceChannelConfigCommandHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<AddDiscordVoiceChannelConfigResponse> Handle(IReceiveContext<AddDiscordVoiceChannelConfigCommand> context, CancellationToken cancellationToken)
    {
        return await _service.AddConfigAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
