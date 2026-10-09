using System.Text.Json;
using Void.Bot.Core.Domain.Social;
using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Core.Services.Social;

public interface ISocialNotificationService : IScope
{
    Task EnqueueAsync(IEnumerable<Guid> recipients, string eventType, Guid resourceId,
        long version, Guid correlationId, CancellationToken cancellationToken);
}

public class SocialNotificationService : ISocialNotificationService
{
    private readonly ISocialOutboxDataProvider _outbox;

    public SocialNotificationService(ISocialOutboxDataProvider outbox)
    {
        _outbox = outbox;
    }

    public async Task EnqueueAsync(IEnumerable<Guid> recipients, string eventType, Guid resourceId,
        long version, Guid correlationId, CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var notification = new SocialNotificationDto
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            ResourceId = resourceId,
            ResourceVersion = version,
            CorrelationId = correlationId,
            OccurredAt = now
        };
        var payload = JsonSerializer.Serialize(notification);
        foreach (var recipient in recipients.Distinct())
        {
            await _outbox.AddAsync(new SocialNotificationOutbox
            {
                Id = Guid.NewGuid(),
                RecipientAccountId = recipient,
                Payload = payload,
                CreatedAt = now,
                NextAttemptAt = now
            }, cancellationToken).ConfigureAwait(false);
        }
    }
}
