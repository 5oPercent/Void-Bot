using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Discord;
using Void.Bot.Message.Commands.Discord;

namespace Void.Bot.Core.Handler.CommandHandler.Discord;

public class UpdateDiscordVoiceChannelConfigCommandHandler : ICommandHandler<UpdateDiscordVoiceChannelConfigCommand, UpdateDiscordVoiceChannelConfigResponse>
{
    private readonly IDiscordVoiceChannelService _service;

    public UpdateDiscordVoiceChannelConfigCommandHandler(IDiscordVoiceChannelService service)
    {
        _service = service;
    }

    public async Task<UpdateDiscordVoiceChannelConfigResponse> Handle(IReceiveContext<UpdateDiscordVoiceChannelConfigCommand> context, CancellationToken cancellationToken)
    {
        return await _service.UpdateConfigAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
