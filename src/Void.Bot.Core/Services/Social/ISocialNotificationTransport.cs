using Void.Bot.Message.Dto.Social;

namespace Void.Bot.Core.Services.Social;

public interface ISocialNotificationTransport
{
    Task SendAsync(Guid accountId, bool eraOnly, SocialNotificationDto notification,
        CancellationToken cancellationToken);
}
