using System.Net;
using System.Text.Json;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Reward.Models;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Services.ERA.Drop;

public sealed partial class EraEquipmentDropService
{
    public async Task<SubmitEraGroundRewardResponse> SubmitAsync(SubmitEraGroundRewardCommand command, CancellationToken cancellationToken)
    {
        if (command.WorldInstanceId == Guid.Empty || command.RequestId == Guid.Empty || command.MemberId == Guid.Empty ||
            command.AuthorityEpoch < 1 || command.SourceType is not (EraRewardSourceType.Monster or EraRewardSourceType.Chest) ||
            command.SourceInstanceId == Guid.Empty || command.TriggerIndex < 1 || string.IsNullOrWhiteSpace(command.SourceDefinitionId) ||
            command.SourceDefinitionId.Length > 128 || command.SourceDefinitionId != command.SourceDefinitionId.Trim() ||
            string.IsNullOrWhiteSpace(command.RewardNodeId) || command.RewardNodeId.Length > 128 || command.RewardNodeId != command.RewardNodeId.Trim() ||
            command.Placement == null || !double.IsFinite(command.Placement.X) || !double.IsFinite(command.Placement.Y) ||
            !double.IsFinite(command.Placement.Z) || !double.IsFinite(command.Placement.YawRadians))
        {
            throw Error(HttpStatusCode.BadRequest, "era.drop_invalid_operation", text => text.InvalidOperation);
        }
        // 此命令不使用 IRequireEraPlayer 预锁当前玩家：必须先协调作用域，再按统一顺序锁全体领取人。
        var actor = await _players.RequireCurrentAsync(false, cancellationToken).ConfigureAwait(false);
        var initial = await _worlds.ReadSnapshotAsync(command.WorldInstanceId, actor.Id, cancellationToken).ConfigureAwait(false);
        if (initial == null || !initial.Members.Any(member => member.PlayerId == actor.Id && member.MemberId == command.MemberId))
        {
            throw Error(HttpStatusCode.NotFound, "era.drop_not_found", text => text.NotFound);
        }
        var payload = JsonSerializer.Serialize(new
        {
            SchemaVersion = 1, Delivery = EraRewardDelivery.Ground, command.WorldInstanceId, command.SourceType,
            command.SourceDefinitionId, command.SourceInstanceId, command.TriggerIndex, command.RewardNodeId, command.Placement
        });
        await _rewards.LockScopeAsync(command.WorldInstanceId, cancellationToken).ConfigureAwait(false);
        var request = await _rewards.GetRequestAsync(command.WorldInstanceId, command.RequestId, cancellationToken).ConfigureAwait(false);
        var previous = request == null
            ? await _rewards.GetEventAsync(command, cancellationToken).ConfigureAwait(false)
            : await _rewards.GetEventAsync(request.EventId, cancellationToken).ConfigureAwait(false);
        var candidates = initial.Members.Where(member => member.PlayerId == actor.Id || previous == null && member.IsContinuing).ToList();
        foreach (var playerId in candidates.Select(member => member.PlayerId).Distinct().OrderBy(id => id))
        {
            if (await _characters.LockPlayerAsync(playerId, cancellationToken).ConfigureAwait(false) == null)
            {
                throw Error(HttpStatusCode.Conflict, "era.drop_members_changed", text => text.MembersChanged);
            }
        }
        var characters = new Dictionary<Guid, EraCharacter>();
        foreach (var member in candidates.OrderBy(member => member.CharacterId))
        {
            var character = await _characters.LockCharacterAsync(member.CharacterId, cancellationToken).ConfigureAwait(false);
            if (character == null || character.PlayerId != member.PlayerId)
            {
                throw Error(HttpStatusCode.Conflict, "era.drop_members_changed", text => text.MembersChanged);
            }
            characters.Add(character.Id, character);
        }
        var context = await _worldService.LockRewardWorldAsync(command, actor.Id, previous != null, cancellationToken).ConfigureAwait(false);
        if (previous != null)
        {
            if (previous.WorldInstanceId != command.WorldInstanceId || previous.ScopeId != command.WorldInstanceId || previous.RequestPayload != payload)
            {
                throw Error(HttpStatusCode.Conflict, "era.drop_idempotency_conflict", text => text.IdempotencyConflict);
            }
            EraGroundRewardReceiptDto? receipt;
            try
            {
                receipt = previous.ResultPayload == null ? null : JsonSerializer.Deserialize<EraGroundRewardReceiptDto>(previous.ResultPayload);
            }
            catch (JsonException)
            {
                receipt = null;
            }
            if (receipt == null || receipt.EventId != previous.Id || receipt.WorldInstanceId != command.WorldInstanceId || receipt.Grants == null ||
                receipt.ConfigurationVersion != previous.ConfigurationVersion || receipt.ManifestVersion != previous.ManifestVersion ||
                receipt.CreatedAt != previous.AcceptedAt || receipt.Grants.Count == 0 ||
                receipt.Grants.Any(grant => grant == null || grant.EquipmentIds == null || grant.DropIds == null ||
                    grant.EquipmentIds.Count != grant.DropIds.Count || grant.GrantId == Guid.Empty || grant.PlayerId == Guid.Empty || grant.CharacterId == Guid.Empty))
            {
                throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
            }
            if (request == null)
            {
                _rewards.AddRequest(new EraRewardRequest { ScopeId = command.WorldInstanceId, RequestId = command.RequestId, EventId = previous.Id });
            }
            return new SubmitEraGroundRewardResponse { Code = HttpStatusCode.OK, Data = receipt, IsReplay = true };
        }
        if (request != null)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_data_unavailable", text => text.DataUnavailable);
        }
        // 新加入的成员未在最初锁定集合中时整笔重试，不能在持有世界锁后反向补锁玩家／角色。
        if (context.Members.Any(member => member.IsContinuing && (!characters.TryGetValue(member.CharacterId, out var character) || character.PlayerId != member.PlayerId)))
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_members_changed", text => text.MembersChanged);
        }
        var clockNow = _clock.GetUtcNow();
        var now = new DateTimeOffset(clockNow.Ticks - clockNow.Ticks % 10, clockNow.Offset);
        if (context.Team.ExpiresAt <= clockNow || !context.Team.Members.Any(member => member.PlayerId == actor.Id &&
            member.MemberId == command.MemberId && member.ExpiresAt > clockNow))
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_members_changed", text => text.MembersChanged);
        }
        var recipients = context.Members.Where(member => member.IsContinuing && member.ActivePlayerId == member.PlayerId &&
            member.ActiveCharacterId == member.CharacterId && !characters[member.CharacterId].IsDisabled &&
            context.Team.Members.Any(live => live.PlayerId == member.PlayerId && live.CharacterId == member.CharacterId &&
                live.MemberId == member.MemberId && live.ExpiresAt > clockNow)).OrderBy(member => member.PlayerId).ToList();
        // 队伍服务管理人数上限；发放只使用实际有效成员，不再另设人数限制。
        if (recipients.Count == 0)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_members_changed", text => text.MembersChanged);
        }
        var configuration = await _rewards.LockConfigurationAsync(false, cancellationToken).ConfigureAwait(false);
        var manifest = await EraEquipmentService.ReadConfigurationAsync(_equipment, true, cancellationToken).ConfigureAwait(false);
        EraRewardRules rules;
        try
        {
            if (configuration == null || configuration.Version < 1 || manifest == null)
            {
                throw new ArgumentException("Ground reward configuration is missing.");
            }
            rules = JsonSerializer.Deserialize<EraRewardRules>(configuration.Payload) ?? throw new ArgumentException("Reward configuration is null.");
            EraRewardConfigurationValidator.Validate(rules);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_configuration_unavailable", text => text.ConfigurationUnavailable);
        }
        var node = rules.Nodes.SingleOrDefault(item => item.SourceType == command.SourceType &&
            item.SourceDefinitionId == command.SourceDefinitionId && item.RewardNodeId == command.RewardNodeId);
        if (node == null)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_source_unavailable", text => text.SourceUnavailable);
        }
        if (node.Delivery != EraRewardDelivery.Ground)
        {
            throw Error(HttpStatusCode.Conflict, "era.drop_delivery_unavailable", text => text.DeliveryUnavailable);
        }
        var pool = rules.Pools.Single(item => item.Id == node.PoolId);
        var definitions = manifest!.Definitions.ToDictionary(item => item.Id);
        var weaponTemplates = manifest.Weapons.Templates.ToDictionary(item => item.Id);
        if (pool.Entries.Any(entry => !definitions.ContainsKey(entry.DefinitionId) &&
            (!weaponTemplates.TryGetValue(entry.DefinitionId, out var template) || !template.IsGenerationEnabled)))
        {
            throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_configuration_unavailable", text => text.ConfigurationUnavailable);
        }
        var result = new EraGroundRewardReceiptDto
        {
            EventId = Guid.NewGuid(), WorldInstanceId = context.World.Id, ConfigurationVersion = configuration!.Version,
            ManifestVersion = manifest.Version, CreatedAt = now
        };
        // 原分享资格取当时有效队伍名单，不要求队友已经加入 SQL 世界；私人发放对象仍须继续参与世界。
        var allowedPlayers = context.Team.Members.Where(member => member.ExpiresAt > clockNow)
            .Select(member => member.PlayerId).Distinct().OrderBy(id => id).ToList();
        var allowedPayload = JsonSerializer.Serialize(allowedPlayers);
        var rewardEvent = new EraRewardEvent
        {
            Id = result.EventId, ScopeId = context.World.Id, WorldInstanceId = context.World.Id, SourceType = command.SourceType,
            SourceInstanceId = command.SourceInstanceId, SourceDefinitionId = command.SourceDefinitionId, TriggerIndex = command.TriggerIndex,
            RewardNodeId = command.RewardNodeId, RequestPayload = payload, ConfigurationVersion = result.ConfigurationVersion,
            ManifestVersion = result.ManifestVersion, AcceptedAt = now,
            PlanPayload = JsonSerializer.Serialize(new EraGroundRewardPlan { Node = node, Pool = pool, Placement = command.Placement, AllowedPlayerIds = allowedPlayers })
        };
        _rewards.AddEvent(rewardEvent);
        // 配置、成员、房主权限、事件与所有资产在同一个命令事务中固定；空奖同样保存成功回执。
        foreach (var recipient in recipients)
        {
            var grant = new EraGroundRewardGrantReceiptDto { GrantId = Guid.NewGuid(), PlayerId = recipient.PlayerId, CharacterId = recipient.CharacterId };
            var items = new List<EraEquipmentInstance>();
            var armor = new List<EraArmorInstanceData>();
            var weaponInstances = new Dictionary<Guid, EraWeaponInstanceDto>();
            foreach (var entry in _pools.Roll(node, pool))
            {
                for (var index = 0; index < entry.Count; index++)
                {
                    var isWeapon = weaponTemplates.TryGetValue(entry.DefinitionId, out var template);
                    var generated = isWeapon ? null : _armor.Generate(entry.Grade);
                    var item = new EraEquipmentInstance
                    {
                        Id = Guid.NewGuid(), DefinitionId = entry.DefinitionId, DefinitionVersion = manifest.Version,
                        EquipmentType = isWeapon ? EraEquipmentType.Weapon : EraEquipmentType.Armor,
                        Grade = entry.Grade, Location = EraEquipmentLocation.Ground,
                        Version = 1, CreatedAt = now
                    };
                    items.Add(item);
                    if (isWeapon)
                    {
                        if (_weapons == null) throw Error(HttpStatusCode.ServiceUnavailable, "era.drop_configuration_unavailable", text => text.ConfigurationUnavailable);
                        weaponInstances.Add(item.Id, _weapons.Generate(manifest.Weapons.SlotConfigurations.Single(configuration => configuration.Id == template!.SlotConfigurationId)));
                    }
                    else armor.Add(new EraArmorInstanceData
                    {
                        EquipmentId = item.Id, GenerationRuleVersion = generated!.GenerationRuleVersion,
                        Strength = generated.Attributes.Strength, Survival = generated.Attributes.Survival, Mobility = generated.Attributes.Mobility,
                        Focus = generated.Attributes.Focus, Efficiency = generated.Attributes.Efficiency, Resistance = generated.Attributes.Resistance
                    });
                    var drop = new EraEquipmentDrop
                    {
                        Id = Guid.NewGuid(), EquipmentId = item.Id, ActiveEquipmentId = item.Id, WorldInstanceId = context.World.Id,
                        PrivateOwnerPlayerId = recipient.PlayerId, Status = EraEquipmentDropStatus.Available, Version = 1, CreatedAt = now,
                        X = command.Placement.X, Y = command.Placement.Y, Z = command.Placement.Z, YawRadians = command.Placement.YawRadians
                    };
                    _drops.Add(drop, new EraEquipmentShare { EquipmentId = item.Id, AllowedPlayerIdsPayload = allowedPayload, ShareUntil = now.AddHours(1) });
                    grant.EquipmentIds.Add(item.Id);
                    grant.DropIds.Add(drop.Id);
                }
            }
            _rewards.AddGrant(new EraRewardGrant
            {
                Id = grant.GrantId, EventId = result.EventId, PlayerId = recipient.PlayerId, CharacterId = recipient.CharacterId,
                GrantedAt = now, ResultPayload = JsonSerializer.Serialize(grant)
            }, items, armor);
            if (weaponInstances.Count > 0) _rewards.AddWeaponInstances(weaponInstances);
            result.Grants.Add(grant);
        }
        rewardEvent.ResultPayload = JsonSerializer.Serialize(result);
        _rewards.AddRequest(new EraRewardRequest { ScopeId = context.World.Id, RequestId = command.RequestId, EventId = result.EventId });
        if (now > context.World.LastActiveAt)
        {
            context.World.LastActiveAt = now;
        }
        context.World.TimeoutObservedAt = null;
        return new SubmitEraGroundRewardResponse { Code = HttpStatusCode.OK, Data = result };
    }
}
