using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Handler.RequestHandler.Account;

public sealed class GetTwoFactorStatusRequestHandler(IAccountTwoFactorService service)
    : IRequestHandler<GetTwoFactorStatusRequest, GetTwoFactorStatusResponse>
{
    public Task<GetTwoFactorStatusResponse> Handle(
        IReceiveContext<GetTwoFactorStatusRequest> context,
        CancellationToken cancellationToken)
    {
        return service.GetStatusAsync(cancellationToken);
    }
}
