using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Handler.RequestHandler.Account;

public class GetUserAccountRequestHandler : IRequestHandler<GetUserAccountRequest, GetUserAccountResponse>
{
    private readonly IAccountService _service;

    public GetUserAccountRequestHandler(IAccountService service)
    {
        _service = service;
    }
    
    public async Task<GetUserAccountResponse> Handle(IReceiveContext<GetUserAccountRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetUserAccountAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}