using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [InlineData(13)]
    [InlineData(120)]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task One_armor_category_should_share_all_inventory_spaces_without_counting_equipped_items(int count)
    {
        await SeedAsync(count, equipped: true);

        var response = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
                new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));
        var filtered = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterInventoryRequest, GetEraCharacterInventoryResponse>(
                new GetEraCharacterInventoryRequest { CharacterId = _characterId, Slot = EraArmorSlot.Body }, token));

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.TotalInventoryCapacity.ShouldBe(120);
        response.Data.TotalInventoryCount.ShouldBe(count);
        response.Data.InventoryItems.Count.ShouldBe(count);
        response.Data.InventoryPartitions.Single(partition => partition.Slot == EraArmorSlot.Head).Count.ShouldBe(count);
        response.Data.EquippedSlots.Count(slot => slot.Equipment != null).ShouldBe(1);
        filtered.Code.ShouldBe(HttpStatusCode.OK);
        filtered.Data!.InventoryItems.ShouldBeEmpty();
        filtered.Data.TotalInventoryCapacity.ShouldBe(120);
        filtered.Data.TotalInventoryCount.ShouldBe(count);
    }

    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Pickups_should_share_the_last_inventory_space_across_armor_categories()
    {
        await SeedAsync(119);
        await ReadDatabaseAsync(async context =>
        {
            foreach (var item in await context.Set<EraEquipmentInstance>().Where(item => item.CharacterId == _characterId).ToListAsync())
            {
                item.InventorySlot = EraArmorSlot.Body;
                item.DefinitionId = "test.Body";
            }
            await context.SaveChangesAsync();
        });
        await ConfigureRewardsAsync(count: 2, source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync();
        var world = await ReadyDropWorldAsync(team);
        await SubmitGroundAsync(GroundCommand(world, team));
        var drops = (await QueryDropsAsync(world.WorldInstanceId)).Data!.Drops;

        var picked = await PickupDropAsync(PickupCommand(world, drops[0]));

        picked.Data!.State.TotalInventoryCount.ShouldBe(120);
        picked.Data.State.InventoryPartitions.Single(partition => partition.Slot == EraArmorSlot.Body).Count.ShouldBe(119);
        picked.Data.State.InventoryPartitions.Single(partition => partition.Slot == EraArmorSlot.Head).Count.ShouldBe(1);
        var second = PickupCommand(world, drops[1], picked.Data.State.EquipmentVersion);
        (await Should.ThrowAsync<EraRewardException>(() => PickupDropAsync(second))).ErrorCode.ShouldBe("era.drop_inventory_full");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync(item => item.CharacterId == _characterId &&
                item.Location == EraEquipmentLocation.Inventory)).ShouldBe(120);
            (await context.Set<EraEquipmentDrop>().SingleAsync(drop => drop.Id == drops[1].DropId)).Status
                .ShouldBe(EraEquipmentDropStatus.Available);
        });
    }

    [Fact]
    [Trait("Category", "ERARewardIntegration")]
    public async Task Inventory_rewards_should_check_shared_capacity_and_retry_the_same_plan_after_space_is_freed()
    {
        await SeedAsync(119);
        await ReadDatabaseAsync(async context =>
        {
            foreach (var item in await context.Set<EraEquipmentInstance>().Where(item => item.CharacterId == _characterId).ToListAsync())
            {
                item.InventorySlot = EraArmorSlot.Body;
                item.DefinitionId = "test.Body";
            }
            await context.SaveChangesAsync();
        });
        await ConfigureRewardsAsync(count: 2);
        var prepared = await PrepareRewardAsync(RewardCommand());

        (await Should.ThrowAsync<EraRewardException>(() => GrantRewardAsync(prepared.Data))).ErrorCode.ShouldBe("era.reward_inventory_full");

        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraRewardGrant>().CountAsync()).ShouldBe(0);
            (await context.Set<EraEquipmentInstance>().CountAsync(item => item.CharacterId == _characterId)).ShouldBe(119);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(item => item.Id == _inventory[0].Id);
            item.Location = EraEquipmentLocation.Destroyed;
            item.OwnerPlayerId = null;
            item.CharacterId = null;
            item.Version++;
            await context.SaveChangesAsync();
        });
        var granted = await GrantRewardAsync(prepared.Data);
        granted.Data!.Grants.Single().EquipmentIds.Count.ShouldBe(2);
        await ReadDatabaseAsync(async context =>
        {
            var items = await context.Set<EraEquipmentInstance>().Where(item => item.CharacterId == _characterId &&
                item.Location == EraEquipmentLocation.Inventory).ToListAsync();
            items.Count.ShouldBe(120);
            items.Count(item => item.InventorySlot == EraArmorSlot.Body).ShouldBe(118);
            items.Count(item => item.InventorySlot == EraArmorSlot.Head).ShouldBe(2);
        });
    }

    [Theory]
    [InlineData(13)]
    [InlineData(120)]
    [Trait("Category", "ERARewardIntegration")]
    public async Task One_reward_category_should_fill_shared_inventory_without_a_per_slot_limit(int count)
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(count: count);
        var prepared = await PrepareRewardAsync(RewardCommand());

        var granted = await GrantRewardAsync(prepared.Data);

        granted.Data!.Grants.Single().EquipmentIds.Count.ShouldBe(count);
        await ReadDatabaseAsync(async context =>
        {
            var items = await context.Set<EraEquipmentInstance>().Where(item => item.CharacterId == _characterId &&
                item.Location == EraEquipmentLocation.Inventory).ToListAsync();
            items.Count.ShouldBe(count);
            items.ShouldAllBe(item => item.InventorySlot == EraArmorSlot.Head);
        });
        (await GrantRewardAsync(prepared.Data)).IsReplay.ShouldBeTrue();
    }
}
