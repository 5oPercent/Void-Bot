using System.Net;
using System.Text.Json;
using NSubstitute;
using NSubstitute.Extensions;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Player.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

/// <summary>业务层测试使用独立的持久化桩；真实 SQL 中途失败的事务回滚仍需数据库集成验证。</summary>
public class EraEquipmentOperationFixture
{
    private readonly IEraEquipmentDataProvider _data = Substitute.For<IEraEquipmentDataProvider>();
    private readonly IEraDataProvider _players = Substitute.For<IEraDataProvider>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();
    private readonly EraPlayer _player = new EraPlayer { Id = Guid.NewGuid() };
    private readonly EraCharacter _character;
    private readonly EraEquipmentConfiguration _configuration;
    private readonly List<EraEquipmentReadEntry> _stored = new List<EraEquipmentReadEntry>();
    private readonly List<EraEquipmentOperationReceipt> _receipts = new List<EraEquipmentOperationReceipt>();
    private IReadOnlyList<EraEquipmentPositionChange>? _lastChanges;

    public EraEquipmentOperationFixture()
    {
        _character = new EraCharacter { Id = Guid.NewGuid(), PlayerId = _player.Id, Version = 12, EquipmentVersion = 3 };
        _configuration = new EraEquipmentConfiguration
        {
            Version = 7,
            Payload = JsonSerializer.Serialize(new EraEquipmentManifestDto
            {
                Definitions = EraEquipmentRules.ArmorSlots.Select(slot => new EraArmorDefinitionDto
                {
                    Id = Guid.Parse($"10000000-0000-0000-0000-{(int)slot:000000000000}"),
                    Name = "Test armor",
                    CompatibleSlots = new List<EraEquipmentSlot> { slot }
                }).ToList()
            })
        };
        _user.Id.Returns(Guid.NewGuid());
        _players.LockPlayerByAccountIdAsync(_user.Id!.Value, Arg.Any<CancellationToken>()).Returns(_player);
        _players.LockCharacterAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);
        _data.LockConfigurationAsync(Arg.Any<CancellationToken>()).Returns(_configuration);
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(_configuration);
        _data.LockCharacterEquipmentAsync(Arg.Any<EraCharacter>(), Arg.Any<CancellationToken>()).Returns(_ => Snapshot());
        _data.GetOperationReceiptAsync(_player.Id, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(call => _receipts.SingleOrDefault(receipt => receipt.RequestId == call.ArgAt<Guid>(1)));
        _data.ApplyPositionChangesAsync(_character.Id, _player.Id,
            Arg.Any<IReadOnlyList<EraEquipmentPositionChange>>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            _lastChanges = call.ArgAt<IReadOnlyList<EraEquipmentPositionChange>>(2);
            foreach (var change in _lastChanges)
            {
                var equipment = _stored.Single(entry => entry.Equipment.Id == change.EquipmentId).Equipment;
                equipment.Location = change.Location;
                equipment.EquippedSlot = change.EquippedSlot;
                equipment.Version = change.ExpectedVersion + 1;
            }
            return Task.CompletedTask;
        });
        _data.CreateOperationReceiptAsync(Arg.Any<EraEquipmentOperationReceipt>(), Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                _receipts.Add(call.Arg<EraEquipmentOperationReceipt>());
                return Task.CompletedTask;
            });
    }

    private EraEquipmentService Service()
    {
        var players = new EraPlayerContext(_players, _user, _localization, new EraPlayerData());
        return new EraEquipmentService(_data, new EraCharacterContext(_players, players, _localization), _localization);
    }

    private EraEquipmentReadSnapshot Snapshot()
    {
        // 模拟 AsNoTracking，避免业务层预建响应时直接修改持久化桩。
        return JsonSerializer.Deserialize<EraEquipmentReadSnapshot>(JsonSerializer.Serialize(new EraEquipmentReadSnapshot
        {
            CharacterId = _character.Id,
            PlayerId = _player.Id,
            EquipmentVersion = _character.EquipmentVersion,
            IsCharacterDisabled = _character.IsDisabled,
            Entries = _stored
        }))!;
    }

    private EraEquipmentInstance AddItem(EraEquipmentSlot slot = EraEquipmentSlot.Head,
        EraEquipmentLocation location = EraEquipmentLocation.Inventory)
    {
        var equipment = new EraEquipmentInstance
        {
            Id = Guid.NewGuid(), DefinitionId = Guid.Parse($"10000000-0000-0000-0000-{(int)slot:000000000000}"), DefinitionVersion = 1,
            EquipmentType = EraEquipmentType.Armor, Grade = EraEquipmentGrade.Common,
            OwnerPlayerId = _player.Id, CharacterId = _character.Id,
            Location = location,
            EquippedSlot = location == EraEquipmentLocation.Equipped ? slot : null,
            Version = 5, CreatedAt = new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.Zero)
        };
        _stored.Add(new EraEquipmentReadEntry(equipment, new EraArmorInstanceData
        {
            EquipmentId = equipment.Id, GenerationRuleVersion = 42,
            Strength = 120, Survival = 15, Mobility = 9, Focus = 6, Efficiency = 6, Resistance = 3
        }));
        return equipment;
    }

    private EquipEraEquipmentCommand Equip(EraEquipmentInstance item, EraEquipmentSlot slot = EraEquipmentSlot.Head)
    {
        var current = _stored.SingleOrDefault(entry => entry.Equipment.Location == EraEquipmentLocation.Equipped &&
            entry.Equipment.EquippedSlot == slot)?.Equipment;
        return new EquipEraEquipmentCommand
        {
            CharacterId = _character.Id, RequestId = Guid.NewGuid(), EquipmentId = item.Id, Slot = slot,
            ExpectedEquipmentVersion = _character.EquipmentVersion, ExpectedItemVersion = item.Version,
            ExpectedManifestVersion = _configuration.Version,
            ExpectedEquippedEquipmentId = current?.Id, ExpectedEquippedVersion = current?.Version
        };
    }

    private UnequipEraEquipmentCommand Unequip(EraEquipmentInstance item)
    {
        return new UnequipEraEquipmentCommand
        {
            CharacterId = _character.Id, RequestId = Guid.NewGuid(), EquipmentId = item.Id,
            Slot = item.EquippedSlot!.Value,
            ExpectedEquipmentVersion = _character.EquipmentVersion, ExpectedItemVersion = item.Version
        };
    }

    [Fact]
    public async Task Should_equip_empty_slot_and_return_saved_values_with_new_versions()
    {
        var item = AddItem();
        var createdAt = item.CreatedAt;

        var response = await Service().EquipAsync(Equip(item), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.IsReplay.ShouldBeFalse();
        response.Data!.PreviousEquipmentVersion.ShouldBe(3);
        response.Data.State.EquipmentVersion.ShouldBe(4);
        response.Data.State.TotalInventoryCount.ShouldBe(0);
        var equipped = response.Data.State.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Head).Equipment!;
        equipped.Id.ShouldBe(item.Id);
        equipped.Version.ShouldBe(6);
        equipped.GenerationRuleVersion.ShouldBe(42);
        equipped.ArmorAttributes.Strength.ShouldBe(120);
        equipped.CreatedAt.ShouldBe(createdAt);
        item.OwnerPlayerId.ShouldBe(_player.Id);
        item.CharacterId.ShouldBe(_character.Id);
        _character.EquipmentVersion.ShouldBe(4);
        _character.Version.ShouldBe(12);
        _receipts.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_replace_at_shared_inventory_capacity_and_release_old_slot_first()
    {
        var old = AddItem(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped);
        var candidate = AddItem();
        for (var index = 0; index < 119; index++) AddItem();

        var response = await Service().EquipAsync(Equip(candidate), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCount.ShouldBe(120);
        response.Data.State.InventoryItems.Any(item => item.Id == old.Id).ShouldBeTrue();
        response.Data.State.InventoryItems.Any(item => item.Id == candidate.Id).ShouldBeFalse();
        _lastChanges!.Select(change => change.EquipmentId).ShouldBe(new[] { old.Id, candidate.Id });
        if (_lastChanges != null)
        {
            _lastChanges[0].Location.ShouldBe(EraEquipmentLocation.Inventory);
            _lastChanges[0].EquippedSlot.ShouldBeNull();
            _lastChanges[1].Location.ShouldBe(EraEquipmentLocation.Equipped);
        }

        old.Version.ShouldBe(6);
        candidate.Version.ShouldBe(6);
        _character.EquipmentVersion.ShouldBe(4);
    }

    [Fact]
    public async Task Should_unequip_into_120th_space_without_needing_current_definition()
    {
        var old = AddItem(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped);
        for (var index = 0; index < 119; index++) AddItem();
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);

        var response = await Service().UnequipAsync(Unequip(old), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCount.ShouldBe(120);
        response.Data.State.EquippedSlots.All(slot => slot.Equipment == null).ShouldBeTrue();
        response.Data.State.ManifestVersion.ShouldBeNull();
        old.Location.ShouldBe(EraEquipmentLocation.Inventory);
        old.EquippedSlot.ShouldBeNull();
        old.Version.ShouldBe(6);
    }

    [Fact]
    public async Task Full_inventory_should_allow_equipping_an_empty_slot_and_release_one_shared_space()
    {
        var candidate = AddItem();
        for (var index = 0; index < 119; index++) AddItem();

        var response = await Service().EquipAsync(Equip(candidate), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCapacity.ShouldBe(120);
        response.Data.State.TotalInventoryCount.ShouldBe(119);
        candidate.Location.ShouldBe(EraEquipmentLocation.Equipped);
    }

    [Fact]
    public async Task Should_reject_full_inventory_unequip_without_mutation_or_success_receipt()
    {
        var old = AddItem(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped);
        for (var index = 0; index < 120; index++) AddItem();

        var response = await Service().UnequipAsync(Unequip(old), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_inventory_full");
        old.Location.ShouldBe(EraEquipmentLocation.Equipped);
        old.Version.ShouldBe(5);
        _character.EquipmentVersion.ShouldBe(3);
        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_replace_different_definitions_when_shared_inventory_is_full()
    {
        var old = AddItem(EraEquipmentSlot.Body, EraEquipmentLocation.Equipped);
        old.EquippedSlot = EraEquipmentSlot.Head;
        for (var index = 0; index < 119; index++) AddItem(EraEquipmentSlot.Body);
        var candidate = AddItem();

        var response = await Service().EquipAsync(Equip(candidate), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.TotalInventoryCount.ShouldBe(120);
        _lastChanges.ShouldNotBeNull();
        old.Location.ShouldBe(EraEquipmentLocation.Inventory);
        candidate.Location.ShouldBe(EraEquipmentLocation.Equipped);
    }

    [Fact]
    public async Task Should_replay_original_success_after_later_unequip_without_restoring_asset()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        var first = await service.EquipAsync(command, CancellationToken.None);
        var second = await service.UnequipAsync(Unequip(item), CancellationToken.None);

        var replay = await service.EquipAsync(command, CancellationToken.None);

        first.Data!.State.EquipmentVersion.ShouldBe(4);
        second.Data!.State.EquipmentVersion.ShouldBe(5);
        replay.IsReplay.ShouldBeTrue();
        replay.Data!.State.EquipmentVersion.ShouldBe(4);
        replay.Data.State.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Head).Equipment!.Id.ShouldBe(item.Id);
        item.Location.ShouldBe(EraEquipmentLocation.Inventory);
        item.Version.ShouldBe(7);
        _character.EquipmentVersion.ShouldBe(5);
        _receipts.Count.ShouldBe(2);
        await _data.Received(2).ApplyPositionChangesAsync(_character.Id, _player.Id,
            Arg.Any<IReadOnlyList<EraEquipmentPositionChange>>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_reject_changed_parameters_for_successful_request_id()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        await service.EquipAsync(command, CancellationToken.None);
        command.ExpectedItemVersion++;

        var response = await service.EquipAsync(command, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_idempotency_conflict");
        _character.EquipmentVersion.ShouldBe(4);
        _receipts.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_share_request_id_scope_between_equip_and_unequip()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        await service.EquipAsync(command, CancellationToken.None);
        var unequip = Unequip(item);
        unequip.RequestId = command.RequestId;

        var response = await service.UnequipAsync(unequip, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_idempotency_conflict");
        item.Location.ShouldBe(EraEquipmentLocation.Equipped);
        _receipts.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_share_request_id_scope_across_same_player_characters()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        await service.EquipAsync(command, CancellationToken.None);
        var other = new EraCharacter { Id = Guid.NewGuid(), PlayerId = _player.Id };
        _players.LockCharacterAsync(other.Id, Arg.Any<CancellationToken>()).Returns(other);
        command.CharacterId = other.Id;

        var response = await service.EquipAsync(command, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_idempotency_conflict");
        other.EquipmentVersion.ShouldBe(0);
        _receipts.Count.ShouldBe(1);
    }

    [Fact]
    public async Task Should_reject_stale_character_item_and_target_versions_without_writes()
    {
        var old = AddItem(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped);
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        command.ExpectedEquipmentVersion = 2;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_version_conflict");
        command.ExpectedEquipmentVersion = 3;
        command.ExpectedItemVersion = 4;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_version_conflict");
        command.ExpectedItemVersion = 5;
        command.ExpectedEquippedVersion = 4;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_version_conflict");
        command.ExpectedEquippedVersion = 5;
        command.ExpectedEquippedEquipmentId = Guid.NewGuid();
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_version_conflict");

        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
        old.Version.ShouldBe(5);
        item.Version.ShouldBe(5);
        _character.EquipmentVersion.ShouldBe(3);
    }

    [Fact]
    public async Task Should_reject_candidate_already_equipped_in_another_slot()
    {
        var item = AddItem(EraEquipmentSlot.Body, EraEquipmentLocation.Equipped);

        var response = await Service().EquipAsync(Equip(item), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_version_conflict");
        _lastChanges.ShouldBeNull();
    }

    [Fact]
    public async Task Should_require_matching_manifest_and_compatible_definition_to_equip()
    {
        var item = AddItem(EraEquipmentSlot.Body);
        var command = Equip(item);
        var service = Service();
        command.ExpectedManifestVersion = 6;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_config_version_conflict");
        command.ExpectedManifestVersion = 7;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_incompatible_slot");
        item.DefinitionId = Guid.NewGuid();
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_definition_unavailable");

        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_require_initialized_catalog_for_new_equip()
    {
        var item = AddItem();
        _data.LockConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);

        var response = await Service().EquipAsync(Equip(item), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_config_unavailable");
        _lastChanges.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_disabled_character_but_allow_old_success_replay()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        await service.EquipAsync(command, CancellationToken.None);
        _character.IsDisabled = true;
        _data.LockConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);

        var denied = await service.UnequipAsync(Unequip(item), CancellationToken.None);
        var replay = await service.EquipAsync(command, CancellationToken.None);

        denied.ErrorCode.ShouldBe("era.equipment_character_disabled");
        replay.IsReplay.ShouldBeTrue();
        _character.EquipmentVersion.ShouldBe(4);
        item.Location.ShouldBe(EraEquipmentLocation.Equipped);
    }

    [Fact]
    public async Task Should_not_operate_another_players_character()
    {
        var item = AddItem();
        _character.PlayerId = Guid.NewGuid();

        await Should.ThrowAsync<EraPlayerAccessException>(() => Service().EquipAsync(Equip(item), CancellationToken.None));

        await _data.DidNotReceive().GetOperationReceiptAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
        _lastChanges.ShouldBeNull();
    }

    [Fact]
    public async Task Should_not_accept_equipment_id_outside_current_character_snapshot()
    {
        var command = Equip(new EraEquipmentInstance { Id = Guid.NewGuid(), Version = 5 });

        var response = await Service().EquipAsync(command, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_not_found");
        _lastChanges.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_unpaired_old_slot_expectations_and_unknown_slot()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        command.ExpectedEquippedEquipmentId = Guid.NewGuid();
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_invalid_operation");
        command.ExpectedEquippedEquipmentId = null;
        command.Slot = (EraEquipmentSlot)0;
        (await service.EquipAsync(command, CancellationToken.None)).ErrorCode.ShouldBe("era.equipment_invalid_operation");

        _lastChanges.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_incomplete_saved_attributes_before_any_write()
    {
        var item = AddItem();
        _stored[0] = new EraEquipmentReadEntry(item, null);

        var response = await Service().EquipAsync(Equip(item), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        _lastChanges.ShouldBeNull();
        _receipts.ShouldBeEmpty();
    }

    [Fact]
    public async Task Should_not_overflow_equipment_version()
    {
        var item = AddItem();
        _character.EquipmentVersion = long.MaxValue;

        var response = await Service().EquipAsync(Equip(item), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        _lastChanges.ShouldBeNull();
        item.Version.ShouldBe(5);
    }

    [Fact]
    public async Task Should_throw_on_mid_write_conflict_so_outer_unit_of_work_rolls_back()
    {
        var item = AddItem();
        _data.Configure().ApplyPositionChangesAsync(_character.Id, _player.Id,
                Arg.Any<IReadOnlyList<EraEquipmentPositionChange>>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new EraEquipmentConcurrencyException("simulated conflict")));

        var exception = await Should.ThrowAsync<EraEquipmentWriteConflictException>(() => Service().EquipAsync(Equip(item), CancellationToken.None));

        exception.Code.ShouldBe(HttpStatusCode.Conflict);
        exception.ErrorCode.ShouldBe("era.equipment_version_conflict");
        _character.EquipmentVersion.ShouldBe(3);
        _receipts.ShouldBeEmpty();
        item.Location.ShouldBe(EraEquipmentLocation.Inventory);
    }

    [Fact]
    public async Task Should_not_apply_asset_change_again_if_success_receipt_is_corrupt()
    {
        var item = AddItem();
        var command = Equip(item);
        var service = Service();
        await service.EquipAsync(command, CancellationToken.None);
        _receipts.Single().ResultPayload = "invalid json";

        var response = await service.EquipAsync(command, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        _character.EquipmentVersion.ShouldBe(4);
        item.Version.ShouldBe(6);
        await _data.Received(1).ApplyPositionChangesAsync(_character.Id, _player.Id,
            Arg.Any<IReadOnlyList<EraEquipmentPositionChange>>(), Arg.Any<CancellationToken>());
    }
}
