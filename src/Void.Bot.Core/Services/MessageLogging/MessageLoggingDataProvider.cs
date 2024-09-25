using Void.Bot.Core.Data;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Services.MessageLogging;

public interface IMessageLoggingDataProvider : IScope
{
    Task<PagedList<MessageLog>> GetFilterAsync(
        List<Guid>? ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken);
}

public class MessageLoggingDataProvider(IRepository repository) : IMessageLoggingDataProvider
{
    public async Task<PagedList<MessageLog>> GetFilterAsync(
        List<Guid>? ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = repository.QueryNoTracking<MessageLog>()
            .WhereIF(!ids.IsNullOrEmpty(), x => ids != null && ids.Contains(x.Id))
            .WhereIF(startDate != null, x => x.CreatedDate >= startDate)
            .WhereIF(endDate != null, x => x.CreatedDate <= endDate);
        
        var entities = await query.OrderByDescending(x => x.CreatedDate)
            .ToPagedAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);

        return entities;
    }

    public async Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken)
    {
        await repository.InsertAllAsync(messageLogs, cancellationToken).ConfigureAwait(false);
    }
}