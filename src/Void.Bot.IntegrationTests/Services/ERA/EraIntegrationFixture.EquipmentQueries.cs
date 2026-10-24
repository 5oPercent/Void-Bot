using System.Net;
using System.Text.Json;
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
    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Empty_character_queries_should_return_six_slots_without_creating_assets()
    {
        await SeedAsync(0);

        var manifest = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraEquipmentManifestRequest, GetEraEquipmentManifestResponse>(new GetEraEquipmentManifestRequest(), token));
        var response = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
                new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));

        manifest.Code.ShouldBe(HttpStatusCode.OK);
        manifest.Data!.Version.ShouldBe(7);
        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.EquipmentVersion.ShouldBe(3);
        response.Data.TotalInventoryCount.ShouldBe(0);
        response.Data.TotalInventoryCapacity.ShouldBe(120);
        response.Data.InventoryItems.ShouldBeEmpty();
        response.Data.EquippedSlots.Count.ShouldBe(9);
        response.Data.EquippedSlots.ShouldAllBe(slot => slot.Equipment == null);
        await AssertOriginalStateAsync();
        await ReadDatabaseAsync(async context =>
        {
            // 只有准备的另一玩家装备；GET 连无归属的额外资产也不能创建。
            (await context.Set<EraEquipmentInstance>().CountAsync()).ShouldBe(1);
            (await context.Set<EraArmorInstanceData>().CountAsync()).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Inventory_query_should_isolate_owner_and_location_and_preserve_saved_attributes()
    {
        await SeedAsync(2, equipped: true);
        await ReadDatabaseAsync(async context =>
        {
            AddItems(context, new[] { NewItem(EraEquipmentLocation.Ground), NewItem(EraEquipmentLocation.Warehouse) });
            await context.SaveChangesAsync();
        });

        var response = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterInventoryRequest, GetEraCharacterInventoryResponse>(
                new GetEraCharacterInventoryRequest { CharacterId = _characterId, Slot = EraEquipmentSlot.Head }, token));

        response.Code.ShouldBe(HttpStatusCode.OK);
        var state = response.Data!;
        state.FilterSlot.ShouldBe(EraEquipmentSlot.Head);
        state.TotalInventoryCount.ShouldBe(2);
        state.InventoryItems.Select(item => item.Id).OrderBy(id => id)
            .ShouldBe(_inventory.Select(item => item.Id).OrderBy(id => id));
        state.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Head).Equipment!.Id.ShouldBe(_equipped!.Id);
        foreach (var item in state.InventoryItems)
        {
            item.OwnerPlayerId.ShouldBe(_playerId);
            item.CharacterId.ShouldBe(_characterId);
            item.Location.ShouldBe(EraEquipmentLocation.Inventory);
            item.ArmorAttributes.Strength.ShouldBe(120);
            item.GenerationRuleVersion.ShouldBe(42);
            item.Definition.ShouldNotBeNull();
        }
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Other_players_character_should_be_rejected_by_real_owner_context()
    {
        await SeedAsync();

        var error = await Should.ThrowAsync<EraPlayerAccessException>(() => RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
                new GetEraCharacterEquipmentRequest { CharacterId = _otherCharacterId }, token)));

        error.Code.ShouldBe(HttpStatusCode.NotFound);
        error.ErrorCode.ShouldBe("era.character_not_found");
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Missing_armor_join_row_should_report_corruption_instead_of_hiding_equipment()
    {
        await SeedAsync();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraArmorInstanceData>().Where(row => row.EquipmentId == _inventory[0].Id)
                .ExecuteDeleteAsync()).ShouldBe(1);
        });

        var response = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
                new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));

        response.Code.ShouldBe(HttpStatusCode.ServiceUnavailable);
        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync(row => row.Id == _inventory[0].Id)).ShouldBe(1);
            (await context.Set<EraArmorInstanceData>().CountAsync(row => row.EquipmentId == _inventory[0].Id)).ShouldBe(0);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync()).ShouldBe(0);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(3);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Removed_catalog_definition_should_not_remove_saved_equipment_or_regenerate_attributes()
    {
        await SeedAsync();
        await ReadDatabaseAsync(async context =>
        {
            var configuration = await context.Set<EraEquipmentConfiguration>().SingleAsync(row => row.Id == ConfigurationId);
            configuration.Version = 8;
            configuration.Payload = JsonSerializer.Serialize(new EraEquipmentManifestDto());
            await context.SaveChangesAsync();
        });

        var response = await RequestAsync((mediator, token) =>
            mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
                new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.ManifestVersion.ShouldBe(8);
        var item = response.Data.InventoryItems.ShouldHaveSingleItem();
        item.Id.ShouldBe(_inventory[0].Id);
        item.Definition.ShouldBeNull();
        item.DefinitionId.ShouldBe(Guid.Parse("10000000-0000-0000-0000-000000000001"));
        item.DefinitionVersion.ShouldBe(1);
        item.GenerationRuleVersion.ShouldBe(42);
        item.ArmorAttributes.Strength.ShouldBe(120);
        await AssertOriginalStateAsync();
    }
}
