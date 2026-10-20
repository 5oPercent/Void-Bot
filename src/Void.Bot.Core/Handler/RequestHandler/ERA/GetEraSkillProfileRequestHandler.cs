using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.ERA.Skill;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Handler.RequestHandler.ERA;

public class GetEraSkillProfileRequestHandler : IRequestHandler<GetEraSkillProfileRequest, GetEraSkillProfileResponse>
{
    private readonly IEraSkillService _service;

    public GetEraSkillProfileRequestHandler(IEraSkillService service)
    {
        _service = service;
    }

    public async Task<GetEraSkillProfileResponse> Handle(
        IReceiveContext<GetEraSkillProfileRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service
            .GetProfileAsync(context.Message, cancellationToken)
            .ConfigureAwait(false);
    }
}
