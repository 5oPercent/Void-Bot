using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Security;
using Void.Bot.Message.Request.Security;

namespace Void.Bot.Core.Handler.RequestHandler.Security;

public class GetMenuRequestHandler : IRequestHandler<GetMenuRequest, GetMenuResponse>
{
    private readonly IMenuService _service;

    public GetMenuRequestHandler(IMenuService service)
    {
        _service = service;
    }
    
    public async Task<GetMenuResponse> Handle(IReceiveContext<GetMenuRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetMenuAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}