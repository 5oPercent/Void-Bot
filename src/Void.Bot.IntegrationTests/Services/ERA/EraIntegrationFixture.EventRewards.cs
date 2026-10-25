using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [InlineData(10000, 1)]
    [InlineData(0, 0)]
    [Trait("Category", "ERADropIntegration")]
    public async Task World_event_should_generate_weapon_from_configured_pool_and_replay_without_reroll_or_respawn(int chance, int expectedCount)
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        (await PublishWeaponsAsync(catalog)).Code.ShouldBe(HttpStatusCode.OK);
        await ConfigureRewardsAsync(chance: chance, source: EraRewardSourceType.Event, delivery: EraRewardDelivery.Ground);
        await ReadDatabaseAsync(async context =>
        {
            var row = await context.Set<EraRewardConfiguration>().SingleAsync();
            var rules = JsonSerializer.Deserialize<EraRewardRules>(row.Payload)!;
            rules.Pools[0].Entries.Single().DefinitionId = catalog.Templates[0].Id;
            row.Payload = JsonSerializer.Serialize(rules);
            await context.SaveChangesAsync();
        });
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        command.SourceType = EraRewardSourceType.Event;
        var first = await SubmitGroundAsync(command);
        first.Code.ShouldBe(HttpStatusCode.OK);
        first.Data!.Grants.Single().EquipmentIds.Count.ShouldBe(expectedCount);
        var visible = (await QueryDropsAsync(world.WorldInstanceId)).Data!;
        visible.Drops.Count.ShouldBe(expectedCount);
        if (expectedCount == 1)
        {
            var drop = visible.Drops.Single();
            drop.EquipmentType.ShouldBe(EraEquipmentType.Weapon);
            drop.DefinitionId.ShouldBe(catalog.Templates[0].Id);
            drop.Weapon!.Slots.Count.ShouldBe(catalog.SlotConfigurations[0].Slots.Count);
            drop.Weapon.Slots.Sum(slot => slot.Candidates.Count).ShouldBe(catalog.SlotConfigurations[0].Slots.Sum(slot => slot.CandidateCount));
            (await PickupDropAsync(PickupCommand(world, drop))).Code.ShouldBe(HttpStatusCode.OK);
        }
        await ReadDatabaseAsync(async context =>
        {
            var row = await context.Set<EraRewardConfiguration>().SingleAsync();
            var rules = JsonSerializer.Deserialize<EraRewardRules>(row.Payload)!;
            rules.Nodes.Single().ChanceBasisPoints = 10000;
            row.Payload = JsonSerializer.Serialize(rules);
            row.Version++;
            await context.SaveChangesAsync();
        });
        var replay = await SubmitGroundAsync(command);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops.ShouldBeEmpty();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync(item => item.EquipmentType == EraEquipmentType.Weapon)).ShouldBe(expectedCount);
            (await context.Set<EraRewardEvent>().CountAsync(item => item.SourceType == EraRewardSourceType.Event)).ShouldBe(1);
            if (expectedCount == 1)
                (await context.Set<EraEquipmentInstance>().SingleAsync(item => item.EquipmentType == EraEquipmentType.Weapon)).Location.ShouldBe(EraEquipmentLocation.Inventory);
        });
    }
}
