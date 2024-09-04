using AutoMapper;
using Void.Bot.Core.Domain;
using Void.Bot.Message.Commands.MessageLogging;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Core.Services.MessageLogging;

public interface IMessageLoggingService : IScope
{
    Task<GetMessageLogResponse> GetLogsAsync(GetMessageLogRequest request, CancellationToken cancellationToken);
    
    Task AddLogsAsync(List<AddMessageLoggingCommand> commands, CancellationToken cancellationToken);
}
    
public class MessageLoggingService(IMapper mapper, IMessageLoggingDataProvider dataProvider) : IMessageLoggingService
{
    public async Task<GetMessageLogResponse> GetLogsAsync(GetMessageLogRequest request, CancellationToken cancellationToken)
    {
        return await dataProvider.GetMessageLogsAsync(request.Ids, request.StartDate, request.EndDate, cancellationToken).ConfigureAwait(false);
    }

    public async Task AddLogsAsync(List<AddMessageLoggingCommand> commands, CancellationToken cancellationToken)
    {
        var logs = mapper.Map<List<MessageLog>>(commands);
        
        await dataProvider.AddMessageLogsAsync(logs, cancellationToken).ConfigureAwait(false);
    }
}