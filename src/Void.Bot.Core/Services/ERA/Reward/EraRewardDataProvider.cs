using Void.Bot.Core.Mappings;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward;

public interface IEraRewardDataProvider : IScope
{
    void AddWeaponInstances(IReadOnlyDictionary<Guid, EraWeaponInstanceDto> weapons);
    Task LockScopeAsync(Guid scopeId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> LockEventAsync(Guid eventId, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken);
    Task<EraRewardEvent?> GetEventAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken);
    Task<EraRewardRequest?> GetRequestAsync(Guid scopeId, Guid requestId, CancellationToken cancellationToken);
    Task<EraRewardConfiguration?> LockConfigurationAsync(bool forUpdate, CancellationToken cancellationToken);
    Task<int> LockInventoryCountAsync(Guid characterId, CancellationToken cancellationToken);
    void AddEvent(EraRewardEvent rewardEvent);
    void AddRequest(EraRewardRequest request);
    void AddGrant(EraRewardGrant grant, IEnumerable<EraEquipmentInstance> equipment,
        IEnumerable<EraArmorInstanceData> armor);
}

public sealed class EraRewardDataProvider : IEraRewardDataProvider
{
    private static readonly Guid ConfigurationId = Void.Bot.Message.Constants.ERA.EraRewardConstants.ConfigurationId;
    private readonly IRepository<EraRewardEvent> _eraRewardEventRepository;
    private readonly IRepository<EraRewardRequest> _eraRewardRequestRepository;
    private readonly IRepository<EraRewardConfiguration> _eraRewardConfigurationRepository;
    private readonly IRepository<EraEquipmentInstance> _eraEquipmentInstanceRepository;
    private readonly IRepository<EraRewardGrant> _eraRewardGrantRepository;
    private readonly IRepository<EraArmorInstanceData> _eraArmorInstanceDataRepository;
    private readonly IRepository<EraRewardGrantEquipment> _eraRewardGrantEquipmentRepository;
    private readonly IRepository<EraWeaponModCandidate> _eraWeaponModCandidateRepository;
    private readonly IRepository<EraWeaponModSelection> _eraWeaponModSelectionRepository;

    public EraRewardDataProvider(
        IRepository<EraRewardEvent> eraRewardEventRepository,
        IRepository<EraRewardRequest> eraRewardRequestRepository,
        IRepository<EraRewardConfiguration> eraRewardConfigurationRepository,
        IRepository<EraEquipmentInstance> eraEquipmentInstanceRepository,
        IRepository<EraRewardGrant> eraRewardGrantRepository,
        IRepository<EraArmorInstanceData> eraArmorInstanceDataRepository,
        IRepository<EraRewardGrantEquipment> eraRewardGrantEquipmentRepository,
        IRepository<EraWeaponModCandidate> eraWeaponModCandidateRepository,
        IRepository<EraWeaponModSelection> eraWeaponModSelectionRepository)
    {
        _eraRewardEventRepository = eraRewardEventRepository;
        _eraRewardRequestRepository = eraRewardRequestRepository;
        _eraRewardConfigurationRepository = eraRewardConfigurationRepository;
        _eraEquipmentInstanceRepository = eraEquipmentInstanceRepository;
        _eraRewardGrantRepository = eraRewardGrantRepository;
        _eraArmorInstanceDataRepository = eraArmorInstanceDataRepository;
        _eraRewardGrantEquipmentRepository = eraRewardGrantEquipmentRepository;
        _eraWeaponModCandidateRepository = eraWeaponModCandidateRepository;
        _eraWeaponModSelectionRepository = eraWeaponModSelectionRepository;
    }

    public void AddWeaponInstances(IReadOnlyDictionary<Guid, EraWeaponInstanceDto> weapons)
    {
        foreach (var pair in weapons)
        {
            _eraWeaponModCandidateRepository.InsertMany(EraWeaponDataMapping.CreateCandidates(pair.Key, pair.Value));
            _eraWeaponModSelectionRepository.InsertMany(EraWeaponDataMapping.CreateSelections(pair.Key, pair.Value));
        }
    }

    public async Task LockScopeAsync(Guid scopeId, CancellationToken cancellationToken)
    {
        _eraRewardEventRepository.RequireTransaction();
        await _eraRewardEventRepository.ExecuteSqlAsync(
            $"INSERT INTO era_reward_scope (scope_id) VALUES ({scopeId.ToString()}) ON DUPLICATE KEY UPDATE scope_id = scope_id",
            cancellationToken).ConfigureAwait(false);
    }

    public Task<EraRewardEvent?> GetEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        return _eraRewardEventRepository.QueryNoTracking().SingleOrDefaultAsync(row => row.Id == eventId, cancellationToken);
    }

    public async Task<EraRewardEvent?> LockEventAsync(Guid eventId, CancellationToken cancellationToken)
    {
        _eraRewardEventRepository.RequireTransaction();
        var rows = await _eraRewardEventRepository.QuerySql(
                $"SELECT * FROM era_reward_event WHERE id = {eventId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<EraRewardEvent?> GetEventAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken)
    {
        return _eraRewardEventRepository.Query().SingleOrDefaultAsync(row => row.ScopeId == command.ScopeId && row.SourceType == command.SourceType &&
            row.SourceInstanceId == command.SourceInstanceId && row.TriggerIndex == command.TriggerIndex && row.RewardNodeId == command.RewardNodeId,
            cancellationToken);
    }

    public Task<EraRewardRequest?> GetRequestAsync(Guid scopeId, Guid requestId, CancellationToken cancellationToken)
    {
        return _eraRewardRequestRepository.QueryNoTracking().SingleOrDefaultAsync(row => row.ScopeId == scopeId && row.RequestId == requestId,
            cancellationToken);
    }

    public Task<EraRewardEvent?> GetEventAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken)
    {
        return _eraRewardEventRepository.QueryNoTracking().SingleOrDefaultAsync(row => row.ScopeId == command.WorldInstanceId &&
            row.SourceType == command.SourceType && row.SourceInstanceId == command.SourceInstanceId && row.TriggerIndex == command.TriggerIndex &&
            row.RewardNodeId == command.RewardNodeId, cancellationToken);
    }

    public async Task<EraRewardConfiguration?> LockConfigurationAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        _eraRewardEventRepository.RequireTransaction();
        var query = forUpdate
            ? _eraRewardConfigurationRepository.QuerySql(
                $"SELECT * FROM era_reward_configuration WHERE id = {ConfigurationId.ToString()} FOR UPDATE")
            : _eraRewardConfigurationRepository.QuerySql(
                $"SELECT * FROM era_reward_configuration WHERE id = {ConfigurationId.ToString()} FOR SHARE");
        var rows = await (forUpdate ? query : query.AsNoTracking()).ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public void AddEvent(EraRewardEvent rewardEvent)
    {
        _eraRewardEventRepository.Insert(rewardEvent);
    }

    public void AddRequest(EraRewardRequest request)
    {
        _eraRewardRequestRepository.Insert(request);
    }

    public async Task<int> LockInventoryCountAsync(Guid characterId, CancellationToken cancellationToken)
    {
        _eraRewardEventRepository.RequireTransaction();
        // 上层已锁全部玩家与角色；内部直发背包不操作已穿装备，也不取得世界限制锁。
        var items = await _eraEquipmentInstanceRepository.QuerySql(
                $"SELECT * FROM era_equipment_instance WHERE character_id = {characterId.ToString()} AND location = {(int)EraEquipmentLocation.Inventory} ORDER BY id FOR UPDATE")
            .AsNoTracking().ToListAsync(cancellationToken).ConfigureAwait(false);
        return items.Count;
    }

    public void AddGrant(EraRewardGrant grant, IEnumerable<EraEquipmentInstance> equipment, IEnumerable<EraArmorInstanceData> armor)
    {
        var items = equipment.ToList();
        _eraRewardGrantRepository.Insert(grant);
        _eraEquipmentInstanceRepository.InsertMany(items);
        _eraArmorInstanceDataRepository.InsertMany(armor);
        _eraRewardGrantEquipmentRepository.InsertMany(items.Select(item => new EraRewardGrantEquipment
        {
            GrantId = grant.Id, EquipmentId = item.Id
        }));
    }
}
