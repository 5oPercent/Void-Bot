using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain;
using Void.Bot.Core.Extension;

namespace Void.Bot.Core.Services.MessageLogging;

public interface IMessageLoggingDataProvider : IScope
{
    Task<IEnumerable<MessageLog>> GetFilterAsync(
        List<Guid>? ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default);
    
    Task<PagedList<MessageLog>> GetPageAsync(
        int pageIndex = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken);
}

public class MessageLoggingDataProvider(IRepository repository, IMapper mapper) : IMessageLoggingDataProvider
{
    public async Task<IEnumerable<MessageLog>> GetFilterAsync(
        List<Guid>? ids = default,
        DateTimeOffset? startDate = null,
        DateTimeOffset? endDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = repository.QueryNoTracking<MessageLog>();

        if (!ids.IsNullOrEmpty()) query = query.Where(x => ids != null && ids.Contains(x.Id));
        if (startDate != null) query = query.Where(x => x.CreatedDate >= startDate);
        if (endDate != null) query = query.Where(x => x.CreatedDate <= endDate);
        
        var entities = await query.ToListAsync(cancellationToken);

        return entities;
    }

    public async Task<PagedList<MessageLog>> GetPageAsync(int pageIndex = 1, int pageSize = 20, CancellationToken cancellationToken = default)
    {
        var paged = await repository.QueryNoTracking<MessageLog>().ToPagedAsync(pageIndex, pageSize, cancellationToken).ConfigureAwait(false);

        return paged;
    }

    public async Task AddMessageLogsAsync(List<MessageLog> messageLogs, CancellationToken cancellationToken)
    {
        await repository.InsertAllAsync(messageLogs, cancellationToken).ConfigureAwait(false);
    }
}