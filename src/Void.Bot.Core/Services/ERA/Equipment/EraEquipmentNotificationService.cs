using Void.Bot.Core.Services.Social;
using Void.Bot.Message.Constants.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public interface IEraEquipmentNotificationService : IScope
{
    Task EnqueueAsync(Guid playerId, Guid characterId, long version, Guid requestId, CancellationToken cancellationToken);
}

/// <summary>失效通知与资产同事务入既有 outbox，只有提交后后台投递；通知不携带资产或授权。</summary>
public sealed class EraEquipmentNotificationService : IEraEquipmentNotificationService
{
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly ISocialNotificationService _notifications;

    public EraEquipmentNotificationService(IEraEquipmentDataProvider equipment, ISocialNotificationService notifications)
    {
        _equipment = equipment;
        _notifications = notifications;
    }

    public async Task EnqueueAsync(Guid playerId, Guid characterId, long version, Guid requestId, CancellationToken cancellationToken)
    {
        var accounts = await _equipment.GetNotificationAccountsAsync(playerId, characterId, cancellationToken).ConfigureAwait(false);
        await _notifications.EnqueueAsync(accounts, EraEquipmentConstants.ChangedEventType, characterId, version, requestId, cancellationToken).ConfigureAwait(false);
    }
}
