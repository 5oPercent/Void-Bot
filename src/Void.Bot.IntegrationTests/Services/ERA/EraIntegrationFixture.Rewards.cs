using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Reward.Models;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private static readonly Guid RewardConfigurationId = Guid.Parse("40000000-0000-0000-0000-000000000001");

    private async Task ConfigureRewardsAsync(int count = 1, int chance = 10000,
        EraRewardSourceType source = EraRewardSourceType.Task, EraRewardDelivery delivery = EraRewardDelivery.Inventory)
    {
        await ReadDatabaseAsync(async context =>
        {
            var configuration = await context.Set<EraRewardConfiguration>().SingleOrDefaultAsync(row => row.Id == RewardConfigurationId);
            if (configuration == null)
            {
                configuration = new EraRewardConfiguration { Id = RewardConfigurationId };
                context.Set<EraRewardConfiguration>().Add(configuration);
            }
            configuration.Version = 11;
            configuration.Payload = JsonSerializer.Serialize(new EraRewardRules
            {
                Nodes = new List<EraRewardNode>
                {
                    new EraRewardNode
                    {
                        SourceType = source, SourceDefinitionId = "test.source", RewardNodeId = "complete",
                        PoolId = "test.pool", Delivery = delivery, ChanceBasisPoints = chance, Rolls = 1
                    }
                },
                Pools = new List<EraRewardPool>
                {
                    new EraRewardPool
                    {
                        Id = "test.pool", Entries = new List<EraRewardPoolEntry>
                        {
                            new EraRewardPoolEntry { DefinitionId = "test.Head", Grade = EraArmorGrade.Common, Count = count, Weight = 1 }
                        }
                    }
                }
            });
            await context.SaveChangesAsync();
        });
    }

    private PrepareEraInventoryRewardCommand RewardCommand(bool includeOther = false, EraRewardSourceType source = EraRewardSourceType.Task)
    {
        var command = new PrepareEraInventoryRewardCommand
        {
            ScopeId = Guid.NewGuid(), RequestId = Guid.NewGuid(), SourceType = source,
            SourceInstanceId = Guid.NewGuid(), SourceDefinitionId = "test.source", TriggerIndex = 1, RewardNodeId = "complete",
            Recipients = new List<EraRewardRecipient> { new EraRewardRecipient { PlayerId = _playerId, CharacterId = _characterId } }
        };
        if (includeOther) command.Recipients.Add(new EraRewardRecipient { PlayerId = _otherPlayerId, CharacterId = _otherCharacterId });
        return command;
    }

    private Task<PrepareEraInventoryRewardResponse> PrepareRewardAsync(PrepareEraInventoryRewardCommand command,
        EraIntegrationSqlInterceptor? interceptor = null, CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<PrepareEraInventoryRewardCommand, PrepareEraInventoryRewardResponse>(command, token),
            interceptor, cancellationToken);
    }

    private Task<GrantEraInventoryRewardResponse> GrantRewardAsync(Guid eventId, EraIntegrationSqlInterceptor? interceptor = null,
        CancellationToken cancellationToken = default)
    {
        return RequestAsync((mediator, token) => mediator.SendAsync<GrantEraInventoryRewardCommand, GrantEraInventoryRewardResponse>(
            new GrantEraInventoryRewardCommand { EventId = eventId }, token), interceptor, cancellationToken);
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Reward_batch_should_persist_all_players_equipment_attributes_links_and_receipts()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: 2);
        var prepared = await PrepareRewardAsync(RewardCommand(includeOther: true));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1); // 只有其他玩家的种子装备。
        });
        var response = await GrantRewardAsync(prepared.Data);
        response.Code.ShouldBe(HttpStatusCode.OK);
        response.IsReplay.ShouldBeFalse();
        response.Data!.Grants.Count.ShouldBe(2);
        response.Data.Grants.ShouldAllBe(grant => grant.EquipmentIds.Count == 2 && grant.PreviousEquipmentVersion == 3 && grant.EquipmentVersion == 4);
        await ReadDatabaseAsync(async context =>
        {
            var rewardEvent = await context.Set<EraRewardEvent>().SingleAsync();
            rewardEvent.ConfigurationVersion.ShouldBe(11);
            rewardEvent.ManifestVersion.ShouldBe(7);
            rewardEvent.ResultPayload.ShouldBe(JsonSerializer.Serialize(response.Data));
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(4);
            var ids = response.Data.Grants.SelectMany(grant => grant.EquipmentIds).ToArray();
            var items = await context.Set<EraEquipmentInstance>().Where(item => ids.Contains(item.Id)).ToListAsync();
            items.Count.ShouldBe(4);
            items.ShouldAllBe(item => item.Location == EraEquipmentLocation.Inventory && item.Version == 1 &&
                item.DefinitionVersion == 7 && item.EquippedSlot == null && item.InventorySlot == EraArmorSlot.Head);
            foreach (var grant in response.Data.Grants)
                items.Where(item => grant.EquipmentIds.Contains(item.Id)).ShouldAllBe(item => item.OwnerPlayerId == grant.PlayerId && item.CharacterId == grant.CharacterId);
            var armor = await context.Set<EraArmorInstanceData>().Where(row => ids.Contains(row.EquipmentId)).ToListAsync();
            armor.Count.ShouldBe(4);
            armor.ShouldAllBe(row => row.GenerationRuleVersion == EraArmorGenerationRules.Version &&
                row.Strength + row.Survival + row.Mobility + row.Focus + row.Efficiency + row.Resistance == 60);
            (await context.Set<EraCharacter>().ToListAsync()).ShouldAllBe(character => character.EquipmentVersion == 4 && character.Version == 12);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Reward_identity_should_ignore_request_id_and_recipient_order_but_reject_changed_content()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var command = RewardCommand(includeOther: true);
        var first = await PrepareRewardAsync(command);
        command.Recipients.Reverse();
        (await PrepareRewardAsync(command)).IsReplay.ShouldBeTrue();
        command.RequestId = Guid.NewGuid();
        var alias = await PrepareRewardAsync(command);
        alias.IsReplay.ShouldBeTrue();
        alias.Data.ShouldBe(first.Data);
        command.SourceDefinitionId = "changed";
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_idempotency_conflict");
        command.RequestId = Guid.NewGuid(); // 新请求也不能通过更换来源定义绕过同一业务事件。
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_idempotency_conflict");
        command.SourceDefinitionId = "test.source";
        command.Recipients.RemoveAt(0);
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_idempotency_conflict");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Full_inventory_should_preserve_plan_without_partial_grants_and_retry_without_current_configuration()
    {
        await SeedAsync(120);
        await ConfigureRewardsAsync(count: 2);
        var command = RewardCommand(includeOther: true);
        var prepared = await PrepareRewardAsync(command);
        var planPayload = string.Empty;
        await ReadDatabaseAsync(async context => { planPayload = (await context.Set<EraRewardEvent>().SingleAsync()).PlanPayload; });
        (await Should.ThrowAsync<EraRewardException>(() => GrantRewardAsync(prepared.Data))).ErrorCode.ShouldBe("era.reward_inventory_full");
        await ReadDatabaseAsync(async context =>
        {
            var rewardEvent = await context.Set<EraRewardEvent>().SingleAsync();
            rewardEvent.PlanPayload.ShouldBe(planPayload);
            rewardEvent.ResultPayload.ShouldBeNull();
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(121);
            (await context.Set<EraArmorInstanceData>().CountAsync()).ShouldBe(121);
            (await context.Set<EraCharacter>().ToListAsync()).ShouldAllBe(character => character.EquipmentVersion == 3);
            var configuration = await context.Set<EraRewardConfiguration>().SingleAsync();
            configuration.Version = 12;
            configuration.Payload = "invalid current reward configuration";
            var manifest = await context.Set<EraEquipmentConfiguration>().SingleAsync();
            manifest.Version = 8;
            manifest.Payload = "invalid current manifest";
            // 模拟正常资产流转释放两个格子；不删除历史身份或护甲。
            foreach (var item in await context.Set<EraEquipmentInstance>().Where(item => item.Id == _inventory[0].Id || item.Id == _inventory[1].Id).ToListAsync())
            {
                item.Location = EraEquipmentLocation.Destroyed;
                item.OwnerPlayerId = null;
                item.CharacterId = null;
                item.Version++;
            }
            await context.SaveChangesAsync();
        });
        (await PrepareRewardAsync(command)).Data.ShouldBe(prepared.Data);
        var granted = await GrantRewardAsync(prepared.Data);
        var plan = JsonSerializer.Deserialize<EraInventoryRewardPlan>(planPayload)!;
        granted.Data!.Grants.SelectMany(grant => grant.EquipmentIds).ShouldBe(plan.Grants.SelectMany(grant => grant.Equipment).Select(item => item.EquipmentId));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().SingleAsync()).PlanPayload.ShouldBe(planPayload);
            foreach (var item in plan.Grants.SelectMany(grant => grant.Equipment))
            {
                var saved = await context.Set<EraArmorInstanceData>().SingleAsync(row => row.EquipmentId == item.EquipmentId);
                saved.Strength.ShouldBe(item.Attributes.Strength);
                saved.Resistance.ShouldBe(item.Attributes.Resistance);
                (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == item.EquipmentId)).DefinitionVersion.ShouldBe(7);
            }
            (await context.Set<EraEquipmentInstance>().CountAsync(item => item.CharacterId == _characterId && item.Location == EraEquipmentLocation.Inventory)).ShouldBe(120);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Successful_reward_replay_should_not_restore_destroyed_equipment_or_require_enabled_recipients()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var prepared = await PrepareRewardAsync(RewardCommand());
        var first = await GrantRewardAsync(prepared.Data);
        var equipmentId = first.Data!.Grants.Single().EquipmentIds.Single();
        await ReadDatabaseAsync(async context =>
        {
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == equipmentId);
            item.Location = EraEquipmentLocation.Destroyed;
            item.CharacterId = null;
            item.OwnerPlayerId = null;
            item.Version++;
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).IsDisabled = true;
            await context.SaveChangesAsync();
        });
        var replay = await GrantRewardAsync(prepared.Data);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == equipmentId)).Location.ShouldBe(EraEquipmentLocation.Destroyed);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(2);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(1);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Zero_chance_should_persist_an_empty_result_and_never_reroll_after_configuration_changes()
    {
        await SeedAsync(120);
        await ConfigureRewardsAsync(chance: 0);
        var command = RewardCommand();
        var prepared = await PrepareRewardAsync(command);
        await ConfigureRewardsAsync(chance: 10000);
        (await PrepareRewardAsync(command)).Data.ShouldBe(prepared.Data);
        var granted = await GrantRewardAsync(prepared.Data);
        granted.Data!.Grants.Single().EquipmentIds.ShouldBeEmpty();
        granted.Data.Grants.Single().EquipmentVersion.ShouldBe(3);
        (await GrantRewardAsync(prepared.Data)).IsReplay.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(121);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(0);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
        });
    }

    [Theory]
    [InlineData(EraRewardSourceType.Task)]
    [InlineData(EraRewardSourceType.Monster)]
    [InlineData(EraRewardSourceType.Chest)]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Inventory_delivery_should_depend_on_configuration_instead_of_source_type(EraRewardSourceType source)
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: source);
        var prepared = await PrepareRewardAsync(RewardCommand(source: source));
        (await GrantRewardAsync(prepared.Data)).Data!.Grants.Single().EquipmentIds.Count.ShouldBe(1);
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Ground_delivery_and_unconfigured_sources_should_not_create_inventory_events()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        var command = RewardCommand(source: EraRewardSourceType.Monster);
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_delivery_unavailable");
        command.SourceDefinitionId = "unconfigured";
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_source_unavailable");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(0);
            (await context.Set<EraRewardRequest>().CountAsync()).ShouldBe(0);
            (await context.Set<EraRewardScope>().CountAsync()).ShouldBe(0); // 失败连协调行写入也回滚。
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Reward_link_insert_failure_should_roll_back_assets_versions_and_receipts_but_keep_committed_plan()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: 2);
        var prepared = await PrepareRewardAsync(RewardCommand(includeOther: true));
        var failure = new EraIntegrationSqlInterceptor { FailRewardLinkInsert = true };
        await Should.ThrowAsync<DbUpdateException>(() => GrantRewardAsync(prepared.Data, failure));
        failure.RewardFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().SingleAsync()).ResultPayload.ShouldBeNull();
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraRewardGrantEquipment>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1);
            (await context.Set<EraArmorInstanceData>().CountAsync()).ShouldBe(1);
            (await context.Set<EraCharacter>().ToListAsync()).ShouldAllBe(character => character.EquipmentVersion == 3);
        });
        (await GrantRewardAsync(prepared.Data)).Data!.Grants.Sum(grant => grant.EquipmentIds.Count).ShouldBe(4);
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Configuration_publish_should_reject_stale_versions_and_leave_existing_plans_unchanged()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var prepared = await PrepareRewardAsync(RewardCommand());
        EraRewardRules rules = null!;
        await ReadDatabaseAsync(async context =>
        {
            rules = JsonSerializer.Deserialize<EraRewardRules>((await context.Set<EraRewardConfiguration>().SingleAsync()).Payload)!;
        });
        rules.Nodes.Single().ChanceBasisPoints = 0;
        var publish = new PublishEraRewardConfigurationCommand { ExpectedVersion = 11, Configuration = rules };
        var response = await RequestAsync((mediator, token) =>
            mediator.SendAsync<PublishEraRewardConfigurationCommand, PublishEraRewardConfigurationResponse>(publish, token));
        response.Data.ShouldBe(12);
        (await Should.ThrowAsync<EraRewardException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<PublishEraRewardConfigurationCommand, PublishEraRewardConfigurationResponse>(publish, token))))
            .ErrorCode.ShouldBe("era.reward_configuration_version_conflict");
        var newEvent = await PrepareRewardAsync(RewardCommand());
        (await GrantRewardAsync(prepared.Data)).Data!.Grants.Single().EquipmentIds.Count.ShouldBe(1);
        (await GrantRewardAsync(newEvent.Data)).Data!.Grants.Single().EquipmentIds.ShouldBeEmpty();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardConfiguration>().SingleAsync()).Version.ShouldBe(12);
            var events = await context.Set<EraRewardEvent>().ToListAsync();
            events.Single(row => row.Id == prepared.Data).ConfigurationVersion.ShouldBe(11);
            events.Single(row => row.Id == newEvent.Data).ConfigurationVersion.ShouldBe(12);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Recipient_ownership_and_disabled_state_should_be_checked_before_assets_are_created()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync();
        var command = RewardCommand();
        command.Recipients.Single().CharacterId = _otherCharacterId;
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_recipient_unavailable");
        command.Recipients.Single().CharacterId = _characterId;
        var prepared = await PrepareRewardAsync(command);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).IsDisabled = true;
            await context.SaveChangesAsync();
        });
        (await Should.ThrowAsync<EraRewardException>(() => GrantRewardAsync(prepared.Data))).ErrorCode.ShouldBe("era.reward_recipient_unavailable");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1);
        });
    }
}
