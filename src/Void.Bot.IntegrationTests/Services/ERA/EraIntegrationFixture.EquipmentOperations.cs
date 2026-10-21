using System.Net;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Full_inventory_swap_should_commit_both_positions_and_one_receipt()
    {
        await SeedAsync(120, equipped: true);
        var command = Equip(_inventory[0]);

        var response = await SendEquipAsync(command);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.IsReplay.ShouldBeFalse();
        response.Data!.State.EquipmentVersion.ShouldBe(4);
        response.Data.State.TotalInventoryCount.ShouldBe(120);
        response.Data.State.EquippedSlots.Single(slot => slot.Slot == EraArmorSlot.Head).Equipment!.Id.ShouldBe(command.EquipmentId);
        await ReadDatabaseAsync(async context =>
        {
            var character = await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId);
            character.EquipmentVersion.ShouldBe(4);
            character.Version.ShouldBe(12);
            var items = await context.Set<EraEquipmentInstance>().Where(row => row.CharacterId == _characterId).ToListAsync();
            items.Count.ShouldBe(121);
            items.Count(item => item.Location == EraEquipmentLocation.Inventory).ShouldBe(120);
            var candidate = items.Single(item => item.Id == command.EquipmentId);
            candidate.Location.ShouldBe(EraEquipmentLocation.Equipped);
            candidate.EquippedSlot.ShouldBe(EraArmorSlot.Head);
            candidate.Version.ShouldBe(6);
            var old = items.Single(item => item.Id == _equipped!.Id);
            old.Location.ShouldBe(EraEquipmentLocation.Inventory);
            old.EquippedSlot.ShouldBeNull();
            old.Version.ShouldBe(6);
            foreach (var original in _inventory.Append(_equipped!))
            {
                var item = items.Single(row => row.Id == original.Id);
                item.OwnerPlayerId.ShouldBe(original.OwnerPlayerId);
                item.CreatedAt.ShouldBe(original.CreatedAt);
                var armor = await context.Set<EraArmorInstanceData>().SingleAsync(row => row.EquipmentId == item.Id);
                armor.Strength.ShouldBe(120);
                armor.Survival.ShouldBe(15);
                armor.Mobility.ShouldBe(9);
                armor.Focus.ShouldBe(6);
                armor.Efficiency.ShouldBe(6);
                armor.Resistance.ShouldBe(3);
                armor.GenerationRuleVersion.ShouldBe(42);
                if (item.Id != candidate.Id && item.Id != old.Id) item.Version.ShouldBe(5);
            }
            var receipt = await context.Set<EraEquipmentOperationReceipt>().SingleAsync(row => row.PlayerId == _playerId);
            receipt.RequestId.ShouldBe(command.RequestId);
            receipt.CharacterId.ShouldBe(_characterId);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Full_inventory_unequip_should_leave_all_rows_and_versions_unchanged()
    {
        await SeedAsync(120, equipped: true);

        var response = await SendUnequipAsync(_equipped!.Id);

        response.Code.ShouldBe(HttpStatusCode.Conflict);
        response.ErrorCode.ShouldBe("era.equipment_inventory_full");
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Replaying_equip_after_unequip_should_return_history_without_restoring_equipment()
    {
        await SeedAsync();
        var command = Equip(_inventory[0]);
        (await SendEquipAsync(command)).Code.ShouldBe(HttpStatusCode.OK);
        var unequip = await SendUnequipAsync(command.EquipmentId, equipmentVersion: 4, itemVersion: 6);
        unequip.Code.ShouldBe(HttpStatusCode.OK);
        unequip.Data!.State.EquipmentVersion.ShouldBe(5);

        var replay = await SendEquipAsync(command);

        replay.Code.ShouldBe(HttpStatusCode.OK);
        replay.IsReplay.ShouldBeTrue();
        replay.Data!.State.EquipmentVersion.ShouldBe(4);
        replay.Data.State.EquippedSlots.Single(slot => slot.Slot == EraArmorSlot.Head).Equipment!.Id.ShouldBe(command.EquipmentId);
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(5);
            var item = await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == command.EquipmentId);
            item.Location.ShouldBe(EraEquipmentLocation.Inventory);
            item.EquippedSlot.ShouldBeNull();
            item.Version.ShouldBe(7);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(2);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Second_conditional_update_conflict_should_roll_back_the_first_real_update()
    {
        await SeedAsync(equipped: true);
        var interceptor = new EraIntegrationSqlInterceptor { ForceZeroRowsOnUpdate = 2 };

        var error = await Should.ThrowAsync<EraEquipmentWriteConflictException>(() => SendEquipAsync(Equip(_inventory[0]), interceptor));

        error.Code.ShouldBe(HttpStatusCode.Conflict);
        error.ErrorCode.ShouldBe("era.equipment_version_conflict");
        interceptor.CompletedUpdates.ShouldBe(2);
        interceptor.AffectedRows.ShouldBe(1);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Receipt_save_failure_should_roll_back_both_real_position_updates_and_versions()
    {
        await SeedAsync(equipped: true);
        var interceptor = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };

        var error = await Should.ThrowAsync<DbUpdateException>(() => SendEquipAsync(Equip(_inventory[0]), interceptor));

        error.InnerException.ShouldBeOfType<MySqlException>().Number.ShouldBe(1644);
        interceptor.ReceiptFailureInjected.ShouldBeTrue();
        interceptor.CompletedUpdates.ShouldBe(2);
        interceptor.AffectedRows.ShouldBe(2);
        await AssertOriginalStateAsync();
    }
}
