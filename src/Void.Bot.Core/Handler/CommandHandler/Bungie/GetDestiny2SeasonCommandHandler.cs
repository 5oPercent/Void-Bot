using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Message.Commands.Bungie.Destiny2;

namespace Void.Bot.Core.Handler.CommandHandler.Bungie;

public class GetDestiny2SeasonCommandHandler : ICommandHandler<GetDestiny2SeasonCommand, GetDestiny2SeasonResponse>
{
    private readonly IBungieService _service;

    public GetDestiny2SeasonCommandHandler(IBungieService service)
    {
        _service = service;
    }

    public async Task<GetDestiny2SeasonResponse> Handle(
        IReceiveContext<GetDestiny2SeasonCommand> context, CancellationToken cancellationToken)
    {
        return await _service.GetDestiny2SeasonAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}
