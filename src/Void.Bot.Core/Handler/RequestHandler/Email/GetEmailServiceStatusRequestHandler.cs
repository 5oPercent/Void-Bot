using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.Email;
using Void.Bot.Message.Request.Email;

namespace Void.Bot.Core.Handler.RequestHandler.Email;

public sealed class GetEmailServiceStatusRequestHandler(IEmailService service)
    : IRequestHandler<GetEmailServiceStatusRequest, GetEmailServiceStatusResponse>
{
    public async Task<GetEmailServiceStatusResponse> Handle(
        IReceiveContext<GetEmailServiceStatusRequest> context,
        CancellationToken cancellationToken)
    {
        return await service.GetStatusAsync(
            context.Message, cancellationToken).ConfigureAwait(false);
    }
}
