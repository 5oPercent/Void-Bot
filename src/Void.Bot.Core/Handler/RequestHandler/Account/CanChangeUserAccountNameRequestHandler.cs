using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Account;
using Void.Bot.Message.Request.Account;

namespace Void.Bot.Core.Handler.RequestHandler.Account;

public class CanChangeUserAccountNameRequestHandler
    : IRequestHandler<CanChangeUserAccountNameRequest, CanChangeUserAccountNameResponse>
{
    private readonly IAccountService _service;

    public CanChangeUserAccountNameRequestHandler(IAccountService service)
    {
        _service = service;
    }

    public async Task<CanChangeUserAccountNameResponse> Handle(
        IReceiveContext<CanChangeUserAccountNameRequest> context,
        CancellationToken cancellationToken)
    {
        return await _service.CanChangeUserAccountNameAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
