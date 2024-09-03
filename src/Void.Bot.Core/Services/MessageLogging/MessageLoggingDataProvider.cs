using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.MessageLogging;
using Void.Bot.Message.Request.MessageLogging;

namespace Void.Bot.Core.Services.MessageLogging;

public interface IMessageLoggingDataProvider : IScope
{
    Task<GetMessageLogResponse> GetMessageLogsAsync(
        List<Guid> ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken);
}

public class MessageLoggingDataProvider(IRepository repository, IMapper mapper) : IMessageLoggingDataProvider
{
    public async Task<GetMessageLogResponse> GetMessageLogsAsync(
        List<Guid> ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = repository.QueryNoTracking<MessageLog>();

        if (!ids.IsNullOrEmpty()) query = query.Where(x => ids.Contains(x.Id));
        if (startDate != null) query = query.Where(x => x.CreatedDate >= startDate);
        if (endDate != null) query = query.Where(x => x.CreatedDate <= endDate);
        
        var entities = await query.ToListAsync(cancellationToken);

        var result = mapper.Map<List<MessageLogDto>>(entities);
        
        return new GetMessageLogResponse
        {
            Result = result
        };
    }

    public async Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken)
    {
        await repository.InsertAllAsync(messageLogs, cancellationToken).ConfigureAwait(false);
    }
}