using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Direct_ground_rewards_should_need_no_reward_configuration_and_replay_without_respawning_picked_equipment()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        (await PublishWeaponsAsync(catalog)).Code.ShouldBe(HttpStatusCode.OK);
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraRewardConfiguration>().RemoveRange(await context.Set<EraRewardConfiguration>().ToListAsync());
            await context.SaveChangesAsync();
        });
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var authority = GroundCommand(world, team);
        var command = new GrantEraGroundRewardCommand
        {
            WorldInstanceId = world.WorldInstanceId, RequestId = Guid.NewGuid(), CharacterId = _characterId, MemberId = authority.MemberId,
            AuthorityEpoch = authority.AuthorityEpoch, Placement = authority.Placement,
            Entries = new List<EraEquipmentRewardEntry>
            {
                new EraEquipmentRewardEntry { DefinitionId = catalog.Templates[0].Id, Grade = EraEquipmentGrade.Reliable, Count = 2 },
                new EraEquipmentRewardEntry { DefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000001") }
            }
        };
        Task<SubmitEraGroundRewardResponse> Send() => RequestAsync((mediator, token) =>
            mediator.SendAsync<GrantEraGroundRewardCommand, SubmitEraGroundRewardResponse>(command, token));
        var first = await Send();
        first.Code.ShouldBe(HttpStatusCode.OK);
        first.Data!.ConfigurationVersion.ShouldBe(0);
        first.Data.Grants.Single().PlayerId.ShouldBe(_playerId);
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.ShouldBeEmpty();
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;
        drops.Count.ShouldBe(3);
        var weapons = drops.Where(drop => drop.EquipmentType == EraEquipmentType.Weapon).ToList();
        weapons.Count.ShouldBe(2);
        weapons.ShouldAllBe(drop => drop.Grade == EraEquipmentGrade.Reliable && drop.DefinitionId == catalog.Templates[0].Id &&
            drop.Weapon!.Slots.Count == catalog.SlotConfigurations[0].Slots.Count);
        var picked = weapons[0];
        (await PickupDropAsync(PickupCommand(world, picked))).Code.ShouldBe(HttpStatusCode.OK);
        var replay = await Send();
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.Count.ShouldBe(2);
        command.Entries[0].Count = 3;
        (await Should.ThrowAsync<EraRewardException>(() => Send())).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        command.Entries[0].Count = 2;
        command.CharacterId = _otherCharacterId;
        (await Should.ThrowAsync<EraRewardException>(() => Send())).ErrorCode.ShouldBe("era.drop_idempotency_conflict");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(1);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(4);
            (await context.Set<EraEquipmentInstance>().SingleAsync(item => item.Id == picked.EquipmentId)).Location.ShouldBe(EraEquipmentLocation.Inventory);
        });
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Invalid_direct_template_should_roll_back_the_batch_and_allow_retry_after_correction()
    {
        await SeedAsync(0);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var authority = GroundCommand(world, team);
        var validId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var command = new GrantEraGroundRewardCommand
        {
            WorldInstanceId = world.WorldInstanceId, RequestId = Guid.NewGuid(), CharacterId = _characterId, MemberId = authority.MemberId,
            AuthorityEpoch = authority.AuthorityEpoch, Placement = authority.Placement,
            Entries = new List<EraEquipmentRewardEntry>
            {
                new EraEquipmentRewardEntry { DefinitionId = validId },
                new EraEquipmentRewardEntry { DefinitionId = Guid.NewGuid() }
            }
        };
        Task<SubmitEraGroundRewardResponse> Send() => RequestAsync((mediator, token) =>
            mediator.SendAsync<GrantEraGroundRewardCommand, SubmitEraGroundRewardResponse>(command, token));
        (await Should.ThrowAsync<EraRewardException>(() => Send())).ErrorCode.ShouldBe("era.drop_invalid_operation");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardEvent>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentDrop>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1); // 仅种子中的另一角色装备。
        });
        command.Entries[1].DefinitionId = validId;
        var corrected = await Send();
        corrected.IsReplay.ShouldBeFalse();
        corrected.Data!.Grants.Single().EquipmentIds.Count.ShouldBe(2);
        (await Send()).IsReplay.ShouldBeTrue();
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Direct_inventory_rewards_should_freeze_generated_equipment_without_source_node_or_pool_configuration()
    {
        await SeedAsync(0);
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraRewardConfiguration>().RemoveRange(await context.Set<EraRewardConfiguration>().ToListAsync());
            await context.SaveChangesAsync();
        });
        var command = RewardCommand();
        command.SourceDefinitionId = command.RewardNodeId = string.Empty;
        command.Entries = new List<EraEquipmentRewardEntry>
        {
            new EraEquipmentRewardEntry { DefinitionId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Count = 2 }
        };
        var prepared = await PrepareRewardAsync(command);
        command.Entries[0].Count = 3;
        (await Should.ThrowAsync<EraRewardException>(() => PrepareRewardAsync(command))).ErrorCode.ShouldBe("era.reward_idempotency_conflict");
        command.Entries[0].Count = 2;
        (await PrepareRewardAsync(command)).IsReplay.ShouldBeTrue();
        var first = await GrantRewardAsync(prepared.Data);
        first.Data!.Grants.Single().EquipmentIds.Count.ShouldBe(2);
        var replay = await GrantRewardAsync(prepared.Data);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(3);
            (await context.Set<EraRewardEvent>().SingleAsync()).ConfigurationVersion.ShouldBe(0);
        });
    }
}
