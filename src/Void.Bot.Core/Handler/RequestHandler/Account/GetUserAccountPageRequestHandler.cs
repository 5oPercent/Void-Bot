using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Handler.RequestHandler.Account;

public class GetUserAccountPageRequestHandler : IRequestHandler<GetUserAccountPageRequest, GetUserAccountPageResponse>
{
    private readonly IAccountService _service;
    
    public GetUserAccountPageRequestHandler(IAccountService service)
    {
        _service = service;
    }
    
    public async Task<GetUserAccountPageResponse> Handle(IReceiveContext<GetUserAccountPageRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetUserAccountPageAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}