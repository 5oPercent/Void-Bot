using SystemEnum = System.Enum;
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
    private readonly IEraWeaponGenerator? _weapons;
    private readonly IEraEquipmentNotificationService? _notifications;

    public EraInventoryRewardService(IEraRewardDataProvider rewards, IEraEquipmentDataProvider equipment,
        IEraDataProvider characters, IEraArmorGenerator armor, IEraRewardPoolRoller pools,
        ILocalizationService localization, TimeProvider clock, IEraWeaponGenerator? weapons = null,
        IEraEquipmentNotificationService? notifications = null)
    {
        _rewards = rewards;
        _equipment = equipment;
        _characters = characters;
        _armor = armor;
        _pools = pools;
        _localization = localization;
        _clock = clock;
        _weapons = weapons;
        _notifications = notifications;
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
        // 与准备奖励相同：先奖励配置，再共享锁定装备目录，防止发布引用已删除的武器模板。
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, true, cancellationToken).ConfigureAwait(false);
        if (manifest == null || command.Configuration.Pools.SelectMany(pool => pool.Entries).Any(entry =>
            !manifest.Armors.Templates.Any(definition => definition.Id == entry.TemplateId) &&
            !manifest.Weapons.Templates.Any(template => template.Id == entry.TemplateId)))
            throw Error(HttpStatusCode.Conflict, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        // 只更新独立奖励配置版本，已有事件完全使用自己的快照；不提升任何缓存版本。
        row.Payload = JsonSerializer.Serialize(command.Configuration);
        row.Version++;
        return new PublishEraRewardConfigurationResponse { Code = HttpStatusCode.OK, Data = row.Version };
    }

    public async Task<PrepareEraInventoryRewardResponse> PrepareAsync(PrepareEraInventoryRewardCommand command, CancellationToken cancellationToken)
    {
        if (command.ScopeId == Guid.Empty || command.RequestId == Guid.Empty || command.SourceInstanceId == Guid.Empty ||
            !SystemEnum.IsDefined(command.SourceType) || command.TriggerIndex < 1 ||
            command.Entries == null && (string.IsNullOrWhiteSpace(command.SourceDefinitionId) || command.SourceDefinitionId.Length > 128 ||
            command.SourceDefinitionId != command.SourceDefinitionId.Trim() || string.IsNullOrWhiteSpace(command.RewardNodeId) ||
            command.RewardNodeId.Length > 128 || command.RewardNodeId != command.RewardNodeId.Trim()) ||
            command.Entries != null && (command.Entries.Count == 0 || command.Entries.Count > 120 ||
                command.Entries.Any(entry => entry == null || !new EraEquipmentRewardEntryValidator().Validate(entry).IsValid) ||
                command.Entries.Sum(entry => (long)entry.Count) > EraEquipmentRules.InventoryCapacity) ||
            command.Recipients == null || command.Recipients.Count == 0 ||
            command.Recipients.Any(recipient => recipient == null || recipient.PlayerId == Guid.Empty || recipient.CharacterId == Guid.Empty) ||
            command.Recipients.Select(recipient => recipient.PlayerId).Distinct().Count() != command.Recipients.Count ||
            command.Recipients.Select(recipient => recipient.CharacterId).Distinct().Count() != command.Recipients.Count)
            throw Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation);

        var recipients = command.Recipients.OrderBy(recipient => recipient.PlayerId).ToList();
        var requestPayload = command.Entries != null ? JsonSerializer.Serialize(new
        {
            SchemaVersion = 2, command.ScopeId, command.SourceType, command.SourceInstanceId, command.SourceDefinitionId,
            command.TriggerIndex, command.RewardNodeId, command.Entries,
            Recipients = recipients.Select(recipient => new { recipient.PlayerId, recipient.CharacterId }).ToList()
        }) : JsonSerializer.Serialize(new
        {
            SchemaVersion = 2, command.ScopeId, command.SourceType, command.SourceInstanceId, command.SourceDefinitionId,
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
        // 配置奖励才读取奖励规则；直接条目只依赖已有装备目录。锁顺序保持奖励配置在装备目录之前。
        var configuration = command.Entries == null
            ? await _rewards.LockConfigurationAsync(false, cancellationToken).ConfigureAwait(false) : null;
        EraEquipmentManifestDto? manifest;
        EraRewardNode? node = null;
        EraRewardPool? pool = null;
        try
        {
            manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, true, cancellationToken).ConfigureAwait(false);
            if (manifest == null) throw new ArgumentException("Equipment configuration is missing.");
            if (command.Entries == null)
            {
                if (configuration == null || configuration.Version < 1) throw new ArgumentException("Reward configuration is missing.");
                var rules = JsonSerializer.Deserialize<EraRewardRules>(configuration.Payload) ?? throw new ArgumentException("Reward configuration is null.");
                EraRewardConfigurationValidator.Validate(rules);
                node = rules.Nodes.SingleOrDefault(item => item.SourceType == command.SourceType &&
                    item.SourceDefinitionId == command.SourceDefinitionId && item.RewardNodeId == command.RewardNodeId);
                if (node == null) throw Error(HttpStatusCode.Conflict, "era.reward_source_unavailable", text => text.SourceUnavailable);
                if (node.Delivery != EraRewardDelivery.Inventory)
                    throw Error(HttpStatusCode.Conflict, "era.reward_delivery_unavailable", text => text.DeliveryUnavailable);
                pool = rules.Pools.Single(item => item.Id == node.PoolId);
                var definitions = manifest.Armors.Templates.Where(item => item.IsGenerationEnabled).Select(item => item.Id).ToHashSet();
                var templates = manifest.Weapons.Templates.Where(item => item.IsGenerationEnabled).Select(item => item.Id).ToHashSet();
                if (pool.Entries.Any(entry => !definitions.Contains(entry.TemplateId) && !templates.Contains(entry.TemplateId)) ||
                    (long)pool.Entries.Max(entry => entry.Count) * node.Rolls > EraEquipmentRules.InventoryCapacity)
                    throw new ArgumentException("Reward pool is invalid.");
            }
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        }
        foreach (var recipient in recipients)
        {
            var character = await _characters.GetCharacterByIdAsync(recipient.CharacterId, cancellationToken).ConfigureAwait(false);
            if (character == null || character.PlayerId != recipient.PlayerId || character.IsDisabled)
                throw Error(HttpStatusCode.Conflict, "era.reward_recipient_unavailable", text => text.RecipientUnavailable);
        }

        var now = _clock.GetUtcNow();
        var plan = new EraInventoryRewardPlan
        {
            EventId = Guid.NewGuid(), ConfigurationVersion = configuration?.Version ?? 0,
            ManifestVersion = manifest.Version, AcceptedAt = new DateTimeOffset(now.Ticks - now.Ticks % 10, now.Offset),
            Node = node, Pool = pool, Entries = command.Entries
        };
        foreach (var recipient in recipients)
        {
            var grant = new EraPlannedRewardGrant
            {
                GrantId = Guid.NewGuid(), PlayerId = recipient.PlayerId, CharacterId = recipient.CharacterId
            };
            try
            {
                grant.Equipment = EraRewardEquipmentGenerator.Generate(command.Entries ?? (IEnumerable<EraEquipmentRewardEntry>)_pools.Roll(node!, pool!), manifest, _armor, _weapons);
            }
            catch (ArgumentException)
            {
                throw command.Entries != null
                    ? Error(HttpStatusCode.BadRequest, "era.reward_invalid_operation", text => text.InvalidOperation)
                    : Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
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
                        expected.PlayerId == grant.PlayerId && expected.CharacterId == grant.CharacterId && grant.EquipmentInstanceIds != null &&
                        grant.PreviousEquipmentVersion >= 0 && (expected.Equipment.Count == 0 || grant.PreviousEquipmentVersion < long.MaxValue) &&
                        grant.EquipmentVersion == grant.PreviousEquipmentVersion + (expected.Equipment.Count > 0 ? 1 : 0) &&
                        expected.Equipment.Select(item => item.EquipmentInstanceId).SequenceEqual(grant.EquipmentInstanceIds))))
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

        // 武器计划固化的引用在落库前固定目录锁，避免发布在计划转资产的间隙删掉模板。
        if (plan.Grants.SelectMany(grant => grant.Equipment).Any(item => item.EquipmentType == EraEquipmentType.Weapon) &&
            await _equipment.LockConfigurationAsync(cancellationToken).ConfigureAwait(false) == null)
            throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_configuration_unavailable", text => text.ConfigurationUnavailable);
        // 所有容量与版本检查完成前不创建任何正式装备或成功回执。
        foreach (var grant in plan.Grants.OrderBy(grant => grant.CharacterId))
        {
            var character = characters[grant.CharacterId];
            var count = await _rewards.LockInventoryCountAsync(character.Id, cancellationToken).ConfigureAwait(false);
            if (character.EquipmentVersion < 0 || (grant.Equipment.Count > 0 && character.EquipmentVersion == long.MaxValue))
                throw Error(HttpStatusCode.ServiceUnavailable, "era.reward_data_unavailable", text => text.DataUnavailable);
            if ((long)count + grant.Equipment.Count > EraEquipmentRules.InventoryCapacity)
            {
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
                EquipmentInstanceIds = grant.Equipment.Select(item => item.EquipmentInstanceId).ToList()
            };
            _rewards.AddGrant(new EraRewardGrant
            {
                Id = grant.GrantId, EventId = plan.EventId, PlayerId = grant.PlayerId, CharacterId = grant.CharacterId,
                GrantedAt = result.GrantedAt, ResultPayload = JsonSerializer.Serialize(receipt)
            }, grant.Equipment.Select(item => new EraEquipmentInstance
            {
                Id = item.EquipmentInstanceId, TemplateId = item.TemplateId, GenerationManifestVersion = plan.ManifestVersion,
                EquipmentType = item.EquipmentType, Grade = item.Grade, OwnerPlayerId = grant.PlayerId,
                CharacterId = grant.CharacterId, Location = EraEquipmentLocation.Inventory,
                Version = 1, CreatedAt = result.GrantedAt
            }), grant.Equipment.Where(item => item.EquipmentType == EraEquipmentType.Armor).Select(item => new EraArmorInstance
            {
                EquipmentInstanceId = item.EquipmentInstanceId, GenerationRuleVersion = item.GenerationRuleVersion, Modifications = item.ArmorModifications,
                Strength = item.Attributes.Strength, Survival = item.Attributes.Survival, Mobility = item.Attributes.Mobility,
                Focus = item.Attributes.Focus, Efficiency = item.Attributes.Efficiency, Resistance = item.Attributes.Resistance
            }));
            var weaponInstances = grant.Equipment.Where(item => item.EquipmentType == EraEquipmentType.Weapon)
                .ToDictionary(item => item.EquipmentInstanceId, item => item.Weapon!);
            if (weaponInstances.Count > 0) _rewards.AddWeaponInstances(weaponInstances);
            character.EquipmentVersion = receipt.EquipmentVersion;
            if (grant.Equipment.Count > 0 && _notifications != null)
                await _notifications.EnqueueAsync(grant.PlayerId, grant.CharacterId, receipt.EquipmentVersion, plan.EventId, cancellationToken).ConfigureAwait(false);
            result.Grants.Add(receipt);
        }
        rewardEvent.ResultPayload = JsonSerializer.Serialize(result);
        return new GrantEraInventoryRewardResponse { Code = HttpStatusCode.OK, Data = result };
    }

    private static void ValidatePlan(EraInventoryRewardPlan plan, EraRewardEvent rewardEvent)
    {
        if (plan.SchemaVersion != 2 || plan.EventId != rewardEvent.Id || plan.AcceptedAt != rewardEvent.AcceptedAt ||
            plan.ConfigurationVersion != rewardEvent.ConfigurationVersion || plan.ManifestVersion != rewardEvent.ManifestVersion ||
            plan.ConfigurationVersion < 0 || plan.ManifestVersion < 1 || plan.Grants == null || plan.Grants.Count == 0 ||
            (plan.Entries == null
                ? plan.ConfigurationVersion < 1 || plan.Node == null || plan.Pool == null || plan.Node.SourceType != rewardEvent.SourceType ||
                    plan.Node.SourceDefinitionId != rewardEvent.SourceDefinitionId || plan.Node.RewardNodeId != rewardEvent.RewardNodeId ||
                    plan.Node.PoolId != plan.Pool.Id || plan.Node.Delivery != EraRewardDelivery.Inventory
                : plan.ConfigurationVersion != 0 || plan.Node != null || plan.Pool != null || plan.Entries.Count == 0 ||
                    plan.Entries.Any(entry => entry == null || !new EraEquipmentRewardEntryValidator().Validate(entry).IsValid) ||
                    plan.Entries.Sum(entry => (long)entry.Count) > EraEquipmentRules.InventoryCapacity) ||
            plan.Grants.Any(grant => grant == null || grant.GrantId == Guid.Empty || grant.PlayerId == Guid.Empty ||
                grant.CharacterId == Guid.Empty || grant.Equipment == null || grant.Equipment.Count > EraEquipmentRules.InventoryCapacity) ||
            plan.Grants.Select(grant => grant.PlayerId).Distinct().Count() != plan.Grants.Count ||
            plan.Grants.Select(grant => grant.CharacterId).Distinct().Count() != plan.Grants.Count ||
            plan.Grants.Select(grant => grant.GrantId).Distinct().Count() != plan.Grants.Count)
            throw new ArgumentException("Reward plan header is invalid.");
        var equipmentIds = new HashSet<Guid>();
        foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment))
        {
            if (item == null || item.EquipmentInstanceId == Guid.Empty || !equipmentIds.Add(item.EquipmentInstanceId) ||
                item.TemplateId == Guid.Empty ||
                !SystemEnum.IsDefined(item.Grade) ||
                (item.EquipmentType == EraEquipmentType.Weapon
                    ? !EraEquipmentItemRules.ValidWeapon(item.Weapon)
                    : item.EquipmentType != EraEquipmentType.Armor ||
                        !EraEquipmentItemRules.ValidArmor(item.ArmorModifications) || item.GenerationRuleVersion < 1 || item.Attributes == null || item.Attributes.Strength < 0 ||
                        item.Attributes.Survival < 0 || item.Attributes.Mobility < 0 || item.Attributes.Focus < 0 ||
                        item.Attributes.Efficiency < 0 || item.Attributes.Resistance < 0))
                throw new ArgumentException("Reward plan equipment is invalid.");
        }
    }
}
