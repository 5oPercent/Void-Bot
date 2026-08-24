using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Handler.RequestHandler.Account;

public sealed class GetUserEmailStatusRequestHandler(IAccountService service)
    : IRequestHandler<GetUserEmailStatusRequest, GetUserEmailStatusResponse>
{
    public Task<GetUserEmailStatusResponse> Handle(
        IReceiveContext<GetUserEmailStatusRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetUserEmailStatusAsync(context.Message, cancellationToken);
    }
}
