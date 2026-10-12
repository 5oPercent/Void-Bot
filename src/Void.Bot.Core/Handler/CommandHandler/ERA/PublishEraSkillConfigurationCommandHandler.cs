using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Handler.CommandHandler.ERA;

public class PublishEraSkillConfigurationCommandHandler : ICommandHandler<PublishEraSkillConfigurationCommand, PublishEraSkillConfigurationResponse>
{
    private readonly IEraSkillService _service;

    public PublishEraSkillConfigurationCommandHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<PublishEraSkillConfigurationResponse> Handle(
        IReceiveContext<PublishEraSkillConfigurationCommand> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .PublishAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
