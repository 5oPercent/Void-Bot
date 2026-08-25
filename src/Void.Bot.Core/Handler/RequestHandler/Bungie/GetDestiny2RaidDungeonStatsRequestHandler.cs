using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.RequestHandler.Bungie;

public class GetDestiny2RaidDungeonStatsRequestHandler
    : IRequestHandler<GetDestiny2RaidDungeonStatsRequest, GetDestiny2RaidDungeonStatsResponse>
{
    private readonly IBungieService _service;

    public GetDestiny2RaidDungeonStatsRequestHandler(IBungieService service)
    {
        _service = service;
    }

    public async Task<GetDestiny2RaidDungeonStatsResponse> Handle(
        IReceiveContext<GetDestiny2RaidDungeonStatsRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2RaidDungeonStatsAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
