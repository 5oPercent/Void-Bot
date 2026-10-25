using System.Net;
using Mediator.Net;
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
    private DestroyEraInventoryEquipmentCommand DestroyInventory()
    {
        return new DestroyEraInventoryEquipmentCommand
        {
            CharacterId = _characterId, RequestId = Guid.NewGuid(), ExpectedEquipmentVersion = 3,
            Items = _inventory.Select(item => new EraEquipmentDestructionItemDto
            {
                EquipmentId = item.Id, ExpectedItemVersion = item.Version
            }).ToList()
        };
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Batch_destruction_should_preserve_payloads_and_receipts_and_replay_without_repeating_updates()
    {
        await SeedAsync(2, equipped: true);
        var command = DestroyInventory();
        var response = await RequestAsync((mediator, token) => mediator.SendAsync<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>(command, token));
        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCount.ShouldBe(0);
        response.Data.State.EquipmentVersion.ShouldBe(4);
        command.Items.Reverse();
        var replay = await RequestAsync((mediator, token) => mediator.SendAsync<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>(command, token));
        replay.IsReplay.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            var ids = _inventory.Select(item => item.Id).ToArray();
            var items = await context.Set<EraEquipmentInstance>().Where(row => ids.Contains(row.Id)).ToListAsync();
            items.Count.ShouldBe(2);
            foreach (var item in items)
            {
                item.Location.ShouldBe(EraEquipmentLocation.Destroyed);
                item.OwnerPlayerId.ShouldBeNull();
                item.CharacterId.ShouldBeNull();
                item.EquippedSlot.ShouldBeNull();
                item.Version.ShouldBe(6);
            }
            (await context.Set<EraArmorInstanceData>().CountAsync(row => ids.Contains(row.EquipmentId))).ShouldBe(2);
            (await context.Set<EraEquipmentInstance>().SingleAsync(row => row.Id == _equipped!.Id)).Location.ShouldBe(EraEquipmentLocation.Equipped);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(1);
        });
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Second_destruction_update_conflict_should_roll_back_first_item_and_ownership_changes()
    {
        await SeedAsync(2, equipped: true);
        var command = DestroyInventory();
        var interceptor = new EraIntegrationSqlInterceptor { ForceZeroRowsOnUpdate = 2 };
        await Should.ThrowAsync<EraEquipmentWriteConflictException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>(command, token), interceptor));
        interceptor.CompletedUpdates.ShouldBe(2);
        interceptor.AffectedRows.ShouldBe(1);
        await AssertOriginalStateAsync();
    }

    [Fact]
    [Trait("Category", "ERAEquipmentIntegration")]
    public async Task Destruction_receipt_failure_should_roll_back_all_items_and_character_version()
    {
        await SeedAsync(2, equipped: true);
        var command = DestroyInventory();
        var interceptor = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };
        await Should.ThrowAsync<DbUpdateException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<DestroyEraInventoryEquipmentCommand, DestroyEraInventoryEquipmentResponse>(command, token), interceptor));
        interceptor.ReceiptFailureInjected.ShouldBeTrue();
        interceptor.CompletedUpdates.ShouldBe(2);
        await AssertOriginalStateAsync();
    }
}
