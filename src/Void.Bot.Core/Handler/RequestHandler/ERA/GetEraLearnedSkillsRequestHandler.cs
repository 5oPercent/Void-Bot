using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraLearnedSkillsRequestHandler : IRequestHandler<GetEraLearnedSkillsRequest, GetEraLearnedSkillsResponse>
{
    private readonly IEraSkillService _service;

    public GetEraLearnedSkillsRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GetEraLearnedSkillsResponse> Handle(
        IReceiveContext<GetEraLearnedSkillsRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetLearnedSkillsAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
