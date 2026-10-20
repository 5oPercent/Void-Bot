using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Reward.Models;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Reward;

public interface IEraInventoryRewardService : IScope
{
    Task<PublishEraRewardConfigurationResponse> PublishAsync(PublishEraRewardConfigurationCommand command, CancellationToken cancellationToken);
    Task<PrepareEraInventoryRewardResponse> PrepareAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken);
    Task<GrantEraInventoryRewardResponse> GrantAsync(GrantEraInventoryRewardCommand command, CancellationToken cancellationToken);
}

/// <summary>只供可信后端业务调用。准备计划和正式发放必须分别通过 Mediator 提交，无客户端入口。</summary>
public sealed class EraInventoryRewardService : IEraInventoryRewardService
{
    private readonly IEraRewardDataProvider _rewards;
    private readonly IEraEquipmentDataProvider _equipment;
    private readonly IEraDataProvider _characters;
    private readonly IEraArmorGenerator _armor;
    private readonly IEraRewardPoolRoller _pools;
    private readonly ILocalizationService _localization;
    private readonly TimeProvider _clock;

    public EraInventoryRewardService(IEraRewardDataProvider rewards, IEraEquipmentDataProvider equipment,
        IEraDataProvider characters, IEraArmorGenerator armor, IEraRewardPoolRoller pools,
        ILocalizationService localization, TimeProvider clock)
    {
        _rewards = rewards;
        _equipment = equipment;
        _characters = characters;
        _armor = armor;
        _pools = pools;
        _localization = localization;
        _clock = clock;
    }

    private EraRewardException Error(HttpStatusCode status, string code, Expression<Func<EraRewardContextTemplate, object>> text)
    {
        return new EraRewardException(status, code, _localization.GetValue(text));
    }

    public async Task<PublishEraRewardConfigurationResponse> PublishAsync(PublishEraRewardConfigurationCommand command, CancellationToken cancellationToken)
    {
        if (command.ExpectedVersion < 1 || command.ExpectedVersion == long.MaxValue || command.Configuration == null)
            throw Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation);
        try { EraRewardConfigurationValidator.Validate(command.Configuration); }
        catch (ArgumentException) { throw Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation); }
        var row = await _rewards.LockConfigurationAsync(true, cancellationToken).ConfigureAwait(false);
        if (row == null) throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        if (row.Version != command.ExpectedVersion)
            throw Error(HttpStatusCode.Conflict, "era.reward_configuration_version_conflict", text => text.ConfigurationVersionConflict);
        // 只更新独立奖励配置版本，已有事件完全使用自己的快照；不提升任何缓存版本。
        row.Payload = JsonSerializer.Serialize(command.Configuration);
        row.Version++;
        return new PublishEraRewardConfigurationResponse { Code = HttpStatusCode.OK, Data = row.Version };
    }

    public async Task<PrepareEraInventoryRewardResponse> PrepareAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken)
    {
        if (command.ScopeId == Guid.Empty || command.RequestId == Guid.Empty || command.SourceInstanceId == Guid.Empty ||
            !System.Enum.IsDefined(command.SourceType) || command.TriggerIndex < 1 ||
            string.IsNullOrWhiteSpace(command.SourceDefinitionId) || command.SourceDefinitionId.Length > 128 ||
            command.SourceDefinitionId != command.SourceDefinitionId.Trim() || string.IsNullOrWhiteSpace(command.RewardNodeId) ||
            command.RewardNodeId.Length > 128 || command.RewardNodeId != command.RewardNodeId.Trim() ||
            command.Recipients == null || command.Recipients.Count == 0 ||
            command.Recipients.Any(recipient => recipient == null || recipient.PlayerId == Guid.Empty || recipient.CharacterId == Guid.Empty) ||
            command.Recipients.Select(recipient => recipient.PlayerId).Distinct().Count() != command.Recipients.Count ||
            command.Recipients.Select(recipient => recipient.CharacterId).Distinct().Count() != command.Recipients.Count)
            throw Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation);

        var recipients = command.Recipients.OrderBy(recipient => recipient.PlayerId).ToList();
        var requestPayload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, command.ScopeId, command.SourceType, command.SourceInstanceId, command.SourceDefinitionId,
            command.TriggerIndex, command.RewardNodeId,
            Recipients = recipients.Select(recipient => new { recipient.PlayerId, recipient.CharacterId }).ToList()
        });
        await _rewards.LockScopeAsync(command.ScopeId, cancellationToken).ConfigureAwait(false);
        var request = await _rewards.GetRequestAsync(command.ScopeId, command.RequestId, cancellationToken).ConfigureAwait(false);
        var previous = request == null
            ? await _rewards.GetEventAsync(command, cancellationToken).ConfigureAwait(false)
            : await _rewards.GetEventAsync(request.EventId, cancellationToken).ConfigureAwait(false);
        if (previous != null)
        {
            if (!string.Equals(previous.RequestPayload, requestPayload, StringComparison.Ordinal))
                throw Error(HttpStatusCode.Conflict, "era.reward_idempotency_conflict", text => text.IdempotencyConflict);
            if (request == null) _rewards.AddRequest(new EraRewardRequest
            {
                ScopeId = command.ScopeId, RequestId = command.RequestId, EventId = previous.Id
            });
            return new PrepareEraInventoryRewardResponse { Code = HttpStatusCode.OK, Data = previous.Id, IsReplay = true };
        }
        if (request != null) throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_data_unavailable", text => text.DataUnavailable);
        // 两份配置在事务中加共享锁；首次接受后不再依赖当前配置或当前名单。
        var configuration = await _rewards.LockConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
        var manifestRow = await _equipment.LockConfigurationAsync(cancellationToken).ConfigureAwait(false);
        EraRewardRules rules;
        EraEquipmentManifestDto manifest;
        try
        {
            if (configuration == null || configuration.Version < 1 || manifestRow == null)
                throw new ArgumentException("Reward configuration is missing.");
            rules = JsonSerializer.Deserialize<EraRewardRules>(configuration.Payload)
                ?? throw new ArgumentException("Reward configuration is null.");
            EraRewardConfigurationValidator.Validate(rules);
            manifest = JsonSerializer.Deserialize<EraEquipmentManifestDto>(manifestRow.Payload)
                ?? throw new ArgumentException("Equipment configuration is null.");
            manifest.Version = manifestRow.Version;
            EraEquipmentConfigurationValidator.Validate(manifest);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        }

        var node = rules.Nodes.SingleOrDefault(item => item.SourceType == command.SourceType &&
            item.SourceDefinitionId == command.SourceDefinitionId && item.RewardNodeId == command.RewardNodeId);
        if (node == null) throw Error(HttpStatusCode.Conflict, "era.reward_source_unavailable", text => text.SourceUnavailable);
        // 只看配置的发放方式，不按任务、怪物或宝箱来源限制背包能力。
        if (node.Delivery != EraRewardDelivery.Inventory)
            throw Error(HttpStatusCode.Conflict, "era.reward_delivery_unavailable", text => text.DeliveryUnavailable);
        var pool = rules.Pools.Single(item => item.Id == node.PoolId);
        var definitions = manifest.Definitions.ToDictionary(item => item.Id, StringComparer.Ordinal);
        if (pool.Entries.Any(entry => !definitions.ContainsKey(entry.DefinitionId)))
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        // 拒绝即使空背包也可能永远装不下的配置；普通满包则保留计划供整理后重试。
        if (pool.Entries.GroupBy(entry => definitions[entry.DefinitionId].InventorySlot)
            .Any(group => group.Max(entry => entry.Count) * node.Rolls > 12))
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        foreach (var recipient in recipients)
        {
            var character = await _characters.GetCharacterByIdAsync(recipient.CharacterId, cancellationToken).ConfigureAwait(false);
            if (character == null || character.PlayerId != recipient.PlayerId || character.IsDisabled)
                throw Error(HttpStatusCode.Conflict, "era.reward_recipient_unavailable", text => text.RecipientUnavailable);
        }

        var now = _clock.GetUtcNow();
        var plan = new EraInventoryRewardPlan
        {
            EventId = Guid.NewGuid(), ConfigurationVersion = configuration!.Version,
            ManifestVersion = manifest.Version, AcceptedAt = new DateTimeOffset(now.Ticks - now.Ticks % 10, now.Offset),
            Node = node, Pool = pool
        };
        foreach (var recipient in recipients)
        {
            var grant = new EraPlannedRewardGrant
            {
                GrantId = Guid.NewGuid(), PlayerId = recipient.PlayerId, CharacterId = recipient.CharacterId
            };
            foreach (var entry in _pools.Roll(node, pool))
            {
                for (var index = 0; index < entry.Count; index++)
                {
                    var generated = _armor.Generate(entry.Grade);
                    grant.Equipment.Add(new EraPlannedRewardEquipment
                    {
                        EquipmentId = Guid.NewGuid(), DefinitionId = entry.DefinitionId, Grade = generated.Grade,
                        InventorySlot = definitions[entry.DefinitionId].InventorySlot,
                        GenerationRuleVersion = generated.GenerationRuleVersion, Attributes = generated.Attributes
                    });
                }
            }
            plan.Grants.Add(grant);
        }

        _rewards.AddEvent(new EraRewardEvent
        {
            Id = plan.EventId, ScopeId = command.ScopeId, SourceType = command.SourceType, SourceInstanceId = command.SourceInstanceId,
            SourceDefinitionId = command.SourceDefinitionId, TriggerIndex = command.TriggerIndex, RewardNodeId = command.RewardNodeId,
            RequestPayload = requestPayload, ConfigurationVersion = plan.ConfigurationVersion, ManifestVersion = plan.ManifestVersion,
            AcceptedAt = plan.AcceptedAt, PlanPayload = JsonSerializer.Serialize(plan)
        });
        _rewards.AddRequest(new EraRewardRequest { ScopeId = command.ScopeId, RequestId = command.RequestId, EventId = plan.EventId });
        return new PrepareEraInventoryRewardResponse { Code = HttpStatusCode.OK, Data = plan.EventId };
    }

    public async Task<GrantEraInventoryRewardResponse> GrantAsync(GrantEraInventoryRewardCommand command, CancellationToken cancellationToken)
    {
        if (command.EventId == Guid.Empty) throw Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation);
        var initial = await _rewards.GetEventAsync(command.EventId, cancellationToken).ConfigureAwait(false);
        if (initial == null) throw Error(HttpStatusCode.NotFound, "era.reward_event_not_found", text => text.EventNotFound);
        await _rewards.LockScopeAsync(initial.ScopeId, cancellationToken).ConfigureAwait(false);
        var rewardEvent = await _rewards.LockEventAsync(command.EventId, cancellationToken).ConfigureAwait(false);
        if (rewardEvent == null) throw Error(HttpStatusCode.NotFound, "era.reward_event_not_found", text => text.EventNotFound);
        EraInventoryRewardPlan plan;
        try
        {
            plan = JsonSerializer.Deserialize<EraInventoryRewardPlan>(rewardEvent.PlanPayload)
                ?? throw new ArgumentException("Reward plan is null.");
            ValidatePlan(plan, rewardEvent);
            if (rewardEvent.ResultPayload != null)
            {
                var previous = JsonSerializer.Deserialize<EraInventoryRewardReceipt>(rewardEvent.ResultPayload);
                if (previous == null || previous.EventId != plan.EventId || previous.Grants == null || previous.Grants.Count != plan.Grants.Count ||
                    previous.Grants.Any(grant => grant == null) ||
                    previous.Grants.Select(grant => grant.GrantId).Distinct().Count() != plan.Grants.Count ||
                    previous.Grants.Any(grant => grant == null || !plan.Grants.Any(expected => expected.GrantId == grant.GrantId &&
                        expected.PlayerId == grant.PlayerId && expected.CharacterId == grant.CharacterId && grant.EquipmentIds != null &&
                        grant.PreviousEquipmentVersion >= 0 && (expected.Equipment.Count == 0 || grant.PreviousEquipmentVersion < long.MaxValue) &&
                        grant.EquipmentVersion == grant.PreviousEquipmentVersion + (expected.Equipment.Count > 0 ? 1 : 0) &&
                        expected.Equipment.Select(item => item.EquipmentId).SequenceEqual(grant.EquipmentIds))))
                    throw new ArgumentException("Reward receipt is invalid.");
                return new GrantEraInventoryRewardResponse { Code = HttpStatusCode.OK, Data = previous, IsReplay = true };
            }
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_data_unavailable", text => text.DataUnavailable);
        }

        // 无当前用户预加载锁：内部整批先按固定顺序锁全部玩家，再锁全部角色，避免跨领取人的反向锁。
        foreach (var playerId in plan.Grants.Select(grant => grant.PlayerId).OrderBy(id => id))
        {
            if (await _characters.LockPlayerAsync(playerId, cancellationToken).ConfigureAwait(false) == null)
                throw Error(HttpStatusCode.Conflict, "era.reward_recipient_unavailable", text => text.RecipientUnavailable);
        }
        var characters = new Dictionary<Guid, EraCharacter>();
        foreach (var grant in plan.Grants.OrderBy(grant => grant.CharacterId))
        {
            var character = await _characters.LockCharacterAsync(grant.CharacterId, cancellationToken).ConfigureAwait(false);
            if (character == null || character.PlayerId != grant.PlayerId || character.IsDisabled)
                throw Error(HttpStatusCode.Conflict, "era.reward_recipient_unavailable", text => text.RecipientUnavailable);
            characters.Add(character.Id, character);
        }

        // 所有容量与版本检查完成前不创建任何正式装备或成功回执。
        foreach (var grant in plan.Grants.OrderBy(grant => grant.CharacterId))
        {
            var character = characters[grant.CharacterId];
            var counts = await _rewards.LockInventoryCountsAsync(character.Id, cancellationToken).ConfigureAwait(false);
            if (character.EquipmentVersion < 0 || (grant.Equipment.Count > 0 && character.EquipmentVersion == long.MaxValue))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_data_unavailable", text => text.DataUnavailable);
            foreach (var slot in System.Enum.GetValues<EraArmorSlot>())
            {
                if (counts.GetValueOrDefault(slot) + grant.Equipment.Count(item => item.InventorySlot == slot) > 12)
                    throw Error(HttpStatusCode.Conflict, "era.reward_inventory_full", text => text.InventoryFull);
            }
        }

        var grantedAt = _clock.GetUtcNow();
        var result = new EraInventoryRewardReceipt
        {
            EventId = plan.EventId, GrantedAt = new DateTimeOffset(grantedAt.Ticks - grantedAt.Ticks % 10, grantedAt.Offset)
        };
        foreach (var grant in plan.Grants)
        {
            var character = characters[grant.CharacterId];
            var receipt = new EraInventoryRewardGrantReceipt
            {
                GrantId = grant.GrantId, PlayerId = grant.PlayerId, CharacterId = grant.CharacterId,
                PreviousEquipmentVersion = character.EquipmentVersion,
                EquipmentVersion = character.EquipmentVersion + (grant.Equipment.Count > 0 ? 1 : 0),
                EquipmentIds = grant.Equipment.Select(item => item.EquipmentId).ToList()
            };
            _rewards.AddGrant(new EraRewardGrant
            {
                Id = grant.GrantId, EventId = plan.EventId, PlayerId = grant.PlayerId, CharacterId = grant.CharacterId,
                GrantedAt = result.GrantedAt, ResultPayload = JsonSerializer.Serialize(receipt)
            }, grant.Equipment.Select(item => new EraEquipmentInstance
            {
                Id = item.EquipmentId, DefinitionId = item.DefinitionId, DefinitionVersion = plan.ManifestVersion,
                EquipmentType = EraEquipmentType.Armor, Grade = item.Grade, OwnerPlayerId = grant.PlayerId,
                CharacterId = grant.CharacterId, Location = EraEquipmentLocation.Inventory, InventorySlot = item.InventorySlot,
                Version = 1, CreatedAt = result.GrantedAt
            }), grant.Equipment.Select(item => new EraArmorInstanceData
            {
                EquipmentId = item.EquipmentId, GenerationRuleVersion = item.GenerationRuleVersion,
                Strength = item.Attributes.Strength, Survival = item.Attributes.Survival, Mobility = item.Attributes.Mobility,
                Focus = item.Attributes.Focus, Efficiency = item.Attributes.Efficiency, Resistance = item.Attributes.Resistance
            }));
            character.EquipmentVersion = receipt.EquipmentVersion;
            result.Grants.Add(receipt);
        }
        rewardEvent.ResultPayload = JsonSerializer.Serialize(result);
        return new GrantEraInventoryRewardResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private static void ValidatePlan(EraInventoryRewardPlan plan, EraRewardEvent rewardEvent)
    {
        if (plan.SchemaVersion != 1 || plan.EventId != rewardEvent.Id || plan.AcceptedAt != rewardEvent.AcceptedAt ||
            plan.ConfigurationVersion != rewardEvent.ConfigurationVersion || plan.ManifestVersion != rewardEvent.ManifestVersion ||
            plan.ConfigurationVersion < 1 || plan.ManifestVersion < 1 || plan.Grants == null || plan.Grants.Count == 0 ||
            plan.Node == null || plan.Pool == null || plan.Node.SourceType != rewardEvent.SourceType ||
            plan.Node.SourceDefinitionId != rewardEvent.SourceDefinitionId || plan.Node.RewardNodeId != rewardEvent.RewardNodeId ||
            plan.Node.PoolId != plan.Pool.Id || plan.Node.Delivery != EraRewardDelivery.Inventory ||
            plan.Grants.Any(grant => grant == null || grant.GrantId == Guid.Empty || grant.PlayerId == Guid.Empty ||
                grant.CharacterId == Guid.Empty || grant.Equipment == null || grant.Equipment.Count > 72) ||
            plan.Grants.Select(grant => grant.PlayerId).Distinct().Count() != plan.Grants.Count ||
            plan.Grants.Select(grant => grant.CharacterId).Distinct().Count() != plan.Grants.Count ||
            plan.Grants.Select(grant => grant.GrantId).Distinct().Count() != plan.Grants.Count)
            throw new ArgumentException("Reward plan header is invalid.");
        var equipmentIds = new HashSet<Guid>();
        foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment))
        {
            if (item == null || item.EquipmentId == Guid.Empty || !equipmentIds.Add(item.EquipmentId) ||
                string.IsNullOrWhiteSpace(item.DefinitionId) || item.DefinitionId.Length > 128 ||
                !System.Enum.IsDefined(item.Grade) || !System.Enum.IsDefined(item.InventorySlot) || item.GenerationRuleVersion < 1 ||
                item.Attributes == null || item.Attributes.Strength < 0 || item.Attributes.Survival < 0 || item.Attributes.Mobility < 0 ||
                item.Attributes.Focus < 0 || item.Attributes.Efficiency < 0 || item.Attributes.Resistance < 0)
                throw new ArgumentException("Reward plan equipment is invalid.");
        }
    }
}
