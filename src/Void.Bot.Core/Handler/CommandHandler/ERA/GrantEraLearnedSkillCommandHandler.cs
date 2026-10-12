using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class GrantEraLearnedSkillCommandHandler : ICommandHandler<GrantEraLearnedSkillCommand, GrantEraLearnedSkillResponse>
{
    private readonly IEraSkillService _service;

    public GrantEraLearnedSkillCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GrantEraLearnedSkillResponse> Handle(
        IReceiveContext<GrantEraLearnedSkillCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GrantSkillAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
