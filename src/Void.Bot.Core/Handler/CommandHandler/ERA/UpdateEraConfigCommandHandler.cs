using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class UpdateEraConfigCommandHandler : ICommandHandler<UpdateEraConfigCommand, UpdateEraConfigResponse>
{
    private readonly IEraService _service;

    public UpdateEraConfigCommandHandler(IEraService service)
    {
        _service = service;
    }

    public async Task<UpdateEraConfigResponse> Handle(
        IReceiveContext<UpdateEraConfigCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .UpdateConfigAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
