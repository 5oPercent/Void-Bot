using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class RemoveEraTeamMemberCommandHandler : ICommandHandler<RemoveEraTeamMemberCommand, RemoveEraTeamMemberResponse>
{
    private readonly IEraTeamService _service;
    public RemoveEraTeamMemberCommandHandler(IEraTeamService service)
    {
        _service = service;
    }
    public Task<RemoveEraTeamMemberResponse> Handle(IReceiveContext<RemoveEraTeamMemberCommand> context, CancellationToken cancellationToken)
    {
        return _service.RemoveMemberAsync(context.Message, cancellationToken);
    }
}
