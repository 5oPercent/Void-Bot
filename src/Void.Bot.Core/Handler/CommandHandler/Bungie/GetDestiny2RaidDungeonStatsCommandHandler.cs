using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Commands.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.CommandHandler.Bungie;

public class GetDestiny2RaidDungeonStatsCommandHandler
    : ICommandHandler<GetDestiny2RaidDungeonStatsCommand, GetDestiny2RaidDungeonStatsResponse>
{
    private readonly IBungieService _service;

    public GetDestiny2RaidDungeonStatsCommandHandler(IBungieService service)
    {
        _service = service;
    }

    public async Task<GetDestiny2RaidDungeonStatsResponse> Handle(
        IReceiveContext<GetDestiny2RaidDungeonStatsCommand> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2RaidDungeonStatsAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
