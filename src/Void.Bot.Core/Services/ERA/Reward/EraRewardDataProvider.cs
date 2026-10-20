using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward;

public interface IEraRewardDataProvider : IScope
{
    Task LockScopeAsync(Guid scopeId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> LockEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken);
    Task<EraRewardRequest?> GetRequestAsync(Guid scopeId, Guid requestId, CancellationToken cancellationToken);
    Task<EraRewardConfiguration?> LockConfigurationAsync(bool forUpdate, CancellationToken cancellationToken);
    Task<Dictionary<EraArmorSlot, int>> LockInventoryCountsAsync(Guid characterId, CancellationToken cancellationToken);
    void AddEvent(EraRewardEvent rewardEvent);
    void AddRequest(EraRewardRequest request);
    void AddGrant(EraRewardGrant grant, IEnumerable<EraEquipmentInstance> equipment,
        IEnumerable<EraArmorInstanceData> armor);
}

public sealed class EraRewardDataProvider : IEraRewardDataProvider
{
    private static readonly Guid ConfigurationId = Guid.Parse("40000000-0000-0000-0000-000000000001");
    private readonly VoidBotDbContext _context;

    public EraRewardDataProvider(VoidBotDbContext context)
    {
        _context = context;
    }

    public async Task LockScopeAsync(Guid scopeId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("Reward writes require the command transaction.");
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO era_reward_scope (scope_id) VALUES ({scopeId.ToString()}) ON DUPLICATE KEY UPDATE scope_id = scope_id",
            cancellationToken).ConfigureAwait(false);
    }

    public Task<EraRewardEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return _context.Set<EraRewardEvent>().AsNoTracking().SingleOrDefaultAsync(row => row.Id == eventId, cancellationToken);
    }

    public async Task<EraRewardEvent?> LockEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("Reward event locks require the command transaction.");
        var rows = await _context.Set<EraRewardEvent>().FromSqlInterpolated(
                $"SELECT * FROM era_reward_event WHERE id = {eventId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<EraRewardEvent?> GetEventAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken)
    {
        return _context.Set<EraRewardEvent>().SingleOrDefaultAsync(row => row.ScopeId == command.ScopeId && row.SourceType == command.SourceType &&
            row.SourceInstanceId == command.SourceInstanceId && row.TriggerIndex == command.TriggerIndex && row.RewardNodeId == command.RewardNodeId,
            cancellationToken);
    }

    public Task<EraRewardRequest?> GetRequestAsync(Guid scopeId, Guid requestId, CancellationToken cancellationToken)
    {
        return _context.Set<EraRewardRequest>().AsNoTracking().SingleOrDefaultAsync(row => row.ScopeId == scopeId && row.RequestId == requestId,
            cancellationToken);
    }

    public Task<EraRewardEvent?> GetEventAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken)
    {
        return _context.Set<EraRewardEvent>().AsNoTracking().SingleOrDefaultAsync(row => row.ScopeId == command.WorldInstanceId &&
            row.SourceType == command.SourceType && row.SourceInstanceId == command.SourceInstanceId && row.TriggerIndex == command.TriggerIndex &&
            row.RewardNodeId == command.RewardNodeId, cancellationToken);
    }

    public async Task<EraRewardConfiguration?> LockConfigurationAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("Reward configuration locks require the command transaction.");
        var query = forUpdate
            ? _context.Set<EraRewardConfiguration>().FromSqlInterpolated(
                $"SELECT * FROM era_reward_configuration WHERE id = {ConfigurationId.ToString()} FOR UPDATE")
            : _context.Set<EraRewardConfiguration>().FromSqlInterpolated(
                $"SELECT * FROM era_reward_configuration WHERE id = {ConfigurationId.ToString()} FOR SHARE");
        var rows = await (forUpdate ? query : query.AsNoTracking()).ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public void AddEvent(EraRewardEvent rewardEvent)
    {
        _context.Set<EraRewardEvent>().Add(rewardEvent);
    }

    public void AddRequest(EraRewardRequest request)
    {
        _context.Set<EraRewardRequest>().Add(request);
    }

    public async Task<Dictionary<EraArmorSlot, int>> LockInventoryCountsAsync(Guid characterId, CancellationToken cancellationToken)
    {
        if (_context.Database.CurrentTransaction == null) throw new InvalidOperationException("Reward inventory locks require the command transaction.");
        // 上层已锁全部玩家与角色；内部直发背包不操作已穿装备，也不取得世界限制锁。
        var items = await _context.Set<EraEquipmentInstance>().FromSqlInterpolated(
                $"SELECT * FROM era_equipment_instance WHERE character_id = {characterId.ToString()} AND location = {(int)EraEquipmentLocation.Inventory} ORDER BY id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return items.GroupBy(item => item.InventorySlot).ToDictionary(group => group.Key, group => group.Count());
    }

    public void AddGrant(EraRewardGrant grant, IEnumerable<EraEquipmentInstance> equipment, IEnumerable<EraArmorInstanceData> armor)
    {
        var items = equipment.ToList();
        _context.Set<EraRewardGrant>().Add(grant);
        _context.Set<EraEquipmentInstance>().AddRange(items);
        _context.Set<EraArmorInstanceData>().AddRange(armor);
        _context.Set<EraRewardGrantEquipment>().AddRange(items.Select(item => new EraRewardGrantEquipment
        {
            GrantId = grant.Id, EquipmentId = item.Id
        }));
    }
}
