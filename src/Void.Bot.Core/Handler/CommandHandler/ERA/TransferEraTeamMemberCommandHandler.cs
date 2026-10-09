using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class TransferEraTeamMemberCommandHandler : ICommandHandler<TransferEraTeamMemberCommand, TransferEraTeamMemberResponse>
{
    private readonly IEraTeamService _service;
    public TransferEraTeamMemberCommandHandler(IEraTeamService service)
    {
        _service = service;
    }
    public Task<TransferEraTeamMemberResponse> Handle(IReceiveContext<TransferEraTeamMemberCommand> context, CancellationToken cancellationToken)
    {
        return _service.TransferHostAsync(context.Message, cancellationToken);
    }
}
