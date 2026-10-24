using Microsoft.EntityFrameworkCore;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Equipment;

public partial interface IEraEquipmentDataProvider
{
    Task<List<Guid>> GetNotificationAccountsAsync(Guid playerId, Guid characterId, CancellationToken cancellationToken);
}

public sealed partial class EraEquipmentDataProvider
{
    public async Task<List<Guid>> GetNotificationAccountsAsync(Guid playerId, Guid characterId, CancellationToken cancellationToken)
    {
        var hostPlayers = from member in _eraWorldMemberRepository.Query()
            join world in _eraWorldRepository.Query() on member.WorldId equals world.Id
            where member.ActiveCharacterId == characterId && member.IsContinuing && world.State == EraWorldState.Ready
            select world.HostPlayerId;
        return await _eraPlayerAccountLinkRepository.QueryNoTracking()
            .Where(link => link.PlayerId == playerId || hostPlayers.Contains(link.PlayerId))
            .Select(link => link.AccountId).Distinct().ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
