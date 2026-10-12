using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class SaveEraSkillProfileCommandHandler : ICommandHandler<SaveEraSkillProfileCommand, SaveEraSkillProfileResponse>
{
    private readonly IEraSkillService _service;

    public SaveEraSkillProfileCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<SaveEraSkillProfileResponse> Handle(
        IReceiveContext<SaveEraSkillProfileCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .SaveProfileAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
