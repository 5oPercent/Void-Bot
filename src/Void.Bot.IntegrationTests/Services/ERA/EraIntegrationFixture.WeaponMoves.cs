using System.Net;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [Trait("Category", "ERAEquipmentIntegration")]
    [InlineData(0, false)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(3, false)]
    [InlineData(4, false)]
    [InlineData(0, true)]
    public async Task Equipped_weapon_swap_should_respect_unique_slots_and_roll_back_every_failure_point(int failingUpdate, bool failingReceipt)
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        catalog.Templates[0].SlotConfigurationId = null;
        await PublishWeaponsAsync(catalog);
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid() };
        await ReadDatabaseAsync(async context =>
        {
            context.Set<EraEquipmentInstance>().AddRange(ids.Select((id, index) => new EraEquipmentInstance
            {
                Id = id, DefinitionId = catalog.Templates[0].Id, DefinitionVersion = 1,
                EquipmentType = EraEquipmentType.Weapon, Grade = EraEquipmentGrade.Common,
                CharacterId = _characterId, OwnerPlayerId = _playerId, Version = 5,
                Location = EraEquipmentLocation.Equipped,
                EquippedSlot = index == 0 ? EraEquipmentSlot.Weapon1 : EraEquipmentSlot.Weapon2,
                CreatedAt = DateTimeOffset.UtcNow
            }));
            await context.SaveChangesAsync();
        });
        var command = new EquipEraEquipmentCommand
        {
            CharacterId = _characterId, RequestId = Guid.NewGuid(), EquipmentId = ids[0],
            Slot = EraEquipmentSlot.Weapon2, ExpectedSourceSlot = EraEquipmentSlot.Weapon1,
            ExpectedEquipmentVersion = 3, ExpectedItemVersion = 5, ExpectedManifestVersion = 8,
            ExpectedEquippedEquipmentId = ids[1], ExpectedEquippedVersion = 5
        };
        var interceptor = new EraIntegrationSqlInterceptor
            { ForceZeroRowsOnUpdate = failingUpdate, FailReceiptInsert = failingReceipt };
        bool failed = failingUpdate > 0 || failingReceipt;
        if (failingUpdate > 0)
        {
            var error = await Should.ThrowAsync<EraEquipmentWriteConflictException>(() => SendEquipAsync(command, interceptor));
            error.ErrorCode.ShouldBe("era.equipment_version_conflict");
        }
        else if (failingReceipt)
            await Should.ThrowAsync<DbUpdateException>(() => SendEquipAsync(command, interceptor));
        else
        {
            var response = await SendEquipAsync(command, interceptor);
            response.Code.ShouldBe(HttpStatusCode.OK);
            response.Data!.State.TotalInventoryCount.ShouldBe(0);
            response.Data.State.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Weapon1).Equipment!.Id.ShouldBe(ids[1]);
            response.Data.State.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Weapon2).Equipment!.Id.ShouldBe(ids[0]);
            (await SendEquipAsync(command)).IsReplay.ShouldBeTrue();
        }
        await ReadDatabaseAsync(async context =>
        {
            var items = await context.Set<EraEquipmentInstance>().AsNoTracking().Where(row => ids.Contains(row.Id)).ToListAsync();
            items.Count.ShouldBe(2);
            items.All(item => item.Location == EraEquipmentLocation.Equipped && item.Version == (failed ? 5 : 6)).ShouldBeTrue();
            items.Single(item => item.Id == ids[0]).EquippedSlot.ShouldBe(failed ? EraEquipmentSlot.Weapon1 : EraEquipmentSlot.Weapon2);
            items.Single(item => item.Id == ids[1]).EquippedSlot.ShouldBe(failed ? EraEquipmentSlot.Weapon2 : EraEquipmentSlot.Weapon1);
            (await context.Set<EraCharacter>().SingleAsync(row => row.Id == _characterId)).EquipmentVersion.ShouldBe(failed ? 3 : 4);
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.PlayerId == _playerId)).ShouldBe(failed ? 0 : 1);
        });
    }
}
