using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.MessageLogging;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Core.Handler.RequestHandler.MessageLogging;

public class GetMessageLogPageHandler : IRequestHandler<GetMessageLogPageRequest, GetMessageLogPageResponse>
{
    private readonly IMessageLoggingService _service;

    public GetMessageLogPageHandler(IMessageLoggingService service)
    {
        _service = service;
    }
    
    public async Task<GetMessageLogPageResponse> Handle(IReceiveContext<GetMessageLogPageRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetPageAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}