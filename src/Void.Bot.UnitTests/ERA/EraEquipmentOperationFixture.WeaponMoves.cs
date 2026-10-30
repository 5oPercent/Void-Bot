using System.Net;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public partial class EraEquipmentOperationFixture
{
    private EraWeaponCatalogDto PrepareWeaponMoves(bool occupied, bool returningCompatible = true)
    {
        var catalog = EraWeaponFixture.Catalog();
        catalog.Templates[0].SlotConfigurationId = null;
        catalog.SlotConfigurations.Clear();
        catalog.SlotTypes.Clear();
        catalog.Modifications.Clear();
        var returning = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(catalog.Templates[0]))!;
        returning.Id = Guid.NewGuid();
        if (!returningCompatible) returning.CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Weapon2 };
        catalog.Templates.Add(returning);
        _data.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(catalog);
        for (int index = 0; index < (occupied ? 2 : 1); index++)
        {
            _stored.Add(new EraEquipmentReadEntry(new EraEquipmentInstance
            {
                Id = Guid.NewGuid(), TemplateId = catalog.Templates[index].Id, GenerationManifestVersion = 1,
                EquipmentType = EraEquipmentType.Weapon, Grade = EraEquipmentGrade.Common,
                OwnerPlayerId = _player.Id, CharacterId = _character.Id, Location = EraEquipmentLocation.Equipped,
                EquippedSlot = index == 0 ? EraEquipmentSlot.Weapon1 : EraEquipmentSlot.Weapon2,
                Version = 5, CreatedAt = DateTimeOffset.UtcNow
            }, null, new EraWeaponInstanceDto()));
        }
        return catalog;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_atomically_move_or_swap_equipped_weapons_and_replay_without_writing(bool occupied)
    {
        PrepareWeaponMoves(occupied);
        var source = _stored[0].Equipment;
        var command = Equip(source, EraEquipmentSlot.Weapon2);
        command.ExpectedSourceSlot = EraEquipmentSlot.Weapon1;
        var response = await Service().EquipAsync(command, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCount.ShouldBe(0);
        response.Data.State.EquipmentVersion.ShouldBe(4);
        source.EquippedSlot.ShouldBe(EraEquipmentSlot.Weapon2);
        source.Version.ShouldBe(6);
        if (occupied)
        {
            _stored[1].Equipment.EquippedSlot.ShouldBe(EraEquipmentSlot.Weapon1);
            _stored[1].Equipment.Version.ShouldBe(6);
        }
        _lastChanges!.Count.ShouldBe(occupied ? 2 : 1);
        _receipts.Count.ShouldBe(1);

        var replay = await Service().EquipAsync(command, CancellationToken.None);
        replay.Code.ShouldBe(HttpStatusCode.OK);
        replay.IsReplay.ShouldBeTrue();
        source.Version.ShouldBe(6);
        await _data.Received(1).ApplyPositionChangesAsync(_character.Id, _player.Id,
            Arg.Any<IReadOnlyList<EraEquipmentPositionChange>>(), Arg.Any<CancellationToken>());
        command.ExpectedSourceSlot = null;
        (await Service().EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_idempotency_conflict");
    }

    [Fact]
    public async Task Should_reject_the_entire_swap_when_the_returning_weapon_is_incompatible()
    {
        PrepareWeaponMoves(true, returningCompatible: false);
        var command = Equip(_stored[0].Equipment, EraEquipmentSlot.Weapon2);
        command.ExpectedSourceSlot = EraEquipmentSlot.Weapon1;
        var response = await Service().EquipAsync(command, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_incompatible_slot");
        _stored[0].Equipment.EquippedSlot.ShouldBe(EraEquipmentSlot.Weapon1);
        _stored[1].Equipment.EquippedSlot.ShouldBe(EraEquipmentSlot.Weapon2);
        _stored.All(entry => entry.Equipment.Version == 5).ShouldBeTrue();
        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_reject_locked_source_even_when_the_destination_is_empty(bool occupied)
    {
        PrepareWeaponMoves(occupied);
        _data.LockCharacterEquipmentAsync(Arg.Any<EraCharacter>(), Arg.Any<CancellationToken>()).Returns(_ =>
        {
            var snapshot = Snapshot();
            snapshot.EquippedOperationRestriction = new EraEquippedOperationRestrictionDto
                { WorldInstanceId = Guid.NewGuid(), WorldVersion = 1, RestrictionVersion = 1, IsRestricted = true };
            return snapshot;
        });
        var command = Equip(_stored[0].Equipment, EraEquipmentSlot.Weapon2);
        command.ExpectedSourceSlot = EraEquipmentSlot.Weapon1;
        (await Service().EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_operations_restricted");
        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_reject_stale_source_position_or_item_version_without_partial_changes(bool stalePosition)
    {
        PrepareWeaponMoves(true);
        var command = Equip(_stored[0].Equipment, EraEquipmentSlot.Weapon2);
        command.ExpectedSourceSlot = stalePosition ? EraEquipmentSlot.Weapon3 : EraEquipmentSlot.Weapon1;
        if (!stalePosition) command.ExpectedItemVersion--;
        (await Service().EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_version_conflict");
        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }
}
