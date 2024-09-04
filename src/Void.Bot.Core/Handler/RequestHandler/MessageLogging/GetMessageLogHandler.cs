using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Void.Bot.Core.Services.MessageLogging;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Core.Handler.RequestHandler.MessageLogging;

public class GetMessageLogHandler : IRequestHandler<GetMessageLogRequest, GetMessageLogResponse>
{
    private readonly IMessageLoggingService _service;

    public GetMessageLogHandler(IMessageLoggingService service)
    {
        _service = service;
    }
    
    public async Task<GetMessageLogResponse> Handle(IReceiveContext<GetMessageLogRequest> context, CancellationToken cancellationToken)
    {
        return await _service.GetLogsAsync(context.Message, cancellationToken).ConfigureAwait(false);
    }
}