using System.Net;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.ERA.Player.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraEquipmentServiceFixture
{
    private readonly IEraEquipmentDataProvider _data = Substitute.For<IEraEquipmentDataProvider>();
    private readonly IEraDataProvider _players = Substitute.For<IEraDataProvider>();
    private readonly ICurrentUser _user = Substitute.For<ICurrentUser>();
    private readonly ILocalizationService _localization = Substitute.For<ILocalizationService>();
    private readonly EraPlayer _player = new EraPlayer { Id = Guid.NewGuid() };
    private readonly EraCharacter _character;
    private readonly EraEquipmentReadSnapshot _snapshot;

    public EraEquipmentServiceFixture()
    {
        _character = new EraCharacter { Id = Guid.NewGuid(), PlayerId = _player.Id, Version = 9, EquipmentVersion = 3 };
        _snapshot = new EraEquipmentReadSnapshot
        {
            CharacterId = _character.Id,
            PlayerId = _player.Id,
            EquipmentVersion = 3
        };
        _user.Id.Returns(Guid.NewGuid());
        _players.GetPlayerByAccountIdAsync(_user.Id!.Value, Arg.Any<CancellationToken>()).Returns(_player);
        _players.GetCharacterByIdAsync(_character.Id, Arg.Any<CancellationToken>()).Returns(_character);
        _data.GetCharacterSnapshotAsync(_character.Id, _player.Id, Arg.Any<CancellationToken>()).Returns(_snapshot);
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(Configuration());
        _data.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(_ =>
            JsonSerializer.Deserialize<EraEquipmentManifestDto>(_data.GetConfigurationAsync(CancellationToken.None).GetAwaiter().GetResult()!.Payload)!.Armors);
    }

    private EraEquipmentService Service()
    {
        var players = new EraPlayerContext(_players, _user, _localization, new EraPlayerData());
        return new EraEquipmentService(_data, new EraCharacterContext(_players, players, _localization), _localization);
    }

    [Fact]
    public async Task Should_filter_weapons_and_armor_in_the_same_inventory_without_changing_total_capacity_or_count()
    {
        var catalog = EraWeaponFixture.Catalog();
        var configuration = catalog.SlotConfigurations.Single(item => item.Id == catalog.Templates[0].SlotConfigurationId);
        _data.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(catalog);
        _snapshot.Entries.Add(Entry());
        var weapon = Entry().Equipment;
        weapon.EquipmentType = EraEquipmentType.Weapon;
        weapon.DefinitionId = catalog.Templates[0].Id;
        _snapshot.Entries.Add(new EraEquipmentReadEntry(weapon, null, EraWeaponGenerator.Generate(configuration, _ => 0)));
        var unfiltered = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest { CharacterId = _character.Id }, CancellationToken.None);
        unfiltered.Data!.InventoryItems.Count.ShouldBe(2);
        var filtered = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest
        {
            CharacterId = _character.Id, EquipmentType = EraEquipmentType.Weapon, Slot = EraEquipmentSlot.Weapon2
        }, CancellationToken.None);
        filtered.Data!.InventoryItems.Single().Id.ShouldBe(weapon.Id);
        filtered.Data.TotalInventoryCount.ShouldBe(2);
        filtered.Data.TotalInventoryCapacity.ShouldBe(120);
        filtered.Data.FilterEquipmentType.ShouldBe(EraEquipmentType.Weapon);
        var invalid = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest
        {
            CharacterId = _character.Id, EquipmentType = (EraEquipmentType)100
        }, CancellationToken.None);
        invalid.Code.ShouldBe(HttpStatusCode.BadRequest);
        invalid.ErrorCode.ShouldBe("era.equipment_invalid_type");
    }

    private static EraArmorTemplateDto Definition(Guid? id = null, EraEquipmentSlot slot = EraEquipmentSlot.Head)
    {
        return new EraArmorTemplateDto
        {
            Id = id ?? Guid.Parse("10000000-0000-0000-0000-000000000001"),
            Name = "Same display name", PresentationKey = "armor_test",
            CompatibleSlots = new List<EraEquipmentSlot> { slot }
        };
    }

    private static EraEquipmentConfiguration Configuration()
    {
        return new EraEquipmentConfiguration
        {
            Version = 7,
            Payload = JsonSerializer.Serialize(new EraEquipmentManifestDto
            {
                Version = 999,
                Armors = new EraArmorCatalogDto { Templates = new List<EraArmorTemplateDto> { Definition() } },
                InventoryCapacity = 999,
                GenerationRules = new EraArmorGenerationRulesDto { Version = 999, MaximumDeviation = 999 }
            })
        };
    }

    private EraEquipmentReadEntry Entry(EraEquipmentSlot slot = EraEquipmentSlot.Head,
        EraEquipmentLocation location = EraEquipmentLocation.Inventory)
    {
        var equipment = new EraEquipmentInstance
        {
            Id = Guid.NewGuid(),
            DefinitionId = Guid.Parse($"10000000-0000-0000-0000-{(int)slot:000000000000}"),
            DefinitionVersion = 2,
            EquipmentType = EraEquipmentType.Armor,
            Grade = EraEquipmentGrade.Common,
            OwnerPlayerId = _player.Id,
            CharacterId = _character.Id,
            Location = location,
            EquippedSlot = location == EraEquipmentLocation.Equipped ? slot : null,
            Version = 5,
            CreatedAt = new DateTimeOffset(2026, 10, 20, 0, 0, 0, TimeSpan.Zero)
        };
        return new EraEquipmentReadEntry(equipment, new EraArmorInstanceData
        {
            EquipmentId = equipment.Id,
            GenerationRuleVersion = 42,
            Strength = 120,
            Survival = 15,
            Mobility = 9,
            Focus = 6,
            Efficiency = 6,
            Resistance = 3
        });
    }

    [Fact]
    public async Task Should_use_storage_version_and_server_generation_rules_in_manifest()
    {
        var response = await Service().GetManifestAsync(new GetEraEquipmentManifestRequest(), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.Version.ShouldBe(7);
        response.Data.InventoryCapacity.ShouldBe(120);
        response.Data.GenerationRules.Version.ShouldBe(1);
        response.Data.GenerationRules.MaximumDeviation.ShouldBe(3);
        response.Data.GenerationRules.WeightsPercent.ShouldBe(new[] { 40, 20, 15, 10, 10, 5 });
        response.Data.GenerationRules.GradeBudgets.Select(item => item.Budget).ShouldBe(new[] { 54, 60, 69, 75, 81 });
    }

    [Fact]
    public async Task Should_allow_empty_initialized_catalog()
    {
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new EraEquipmentConfiguration
        {
            Version = 1, Payload = "{\"SchemaVersion\":2}"
        });

        var response = await Service().GetManifestAsync(new GetEraEquipmentManifestRequest(), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.Armors.Templates.ShouldBeEmpty();
        response.Data.GenerationRules.GradeBudgets.Count.ShouldBe(5);
    }

    [Theory]
    [InlineData("not json")]
    [InlineData("null")]
    [InlineData("{\"SchemaVersion\":1}")]
    [InlineData("{\"SchemaVersion\":2,\"Armors\":null}")]
    public async Task Should_report_invalid_catalog_without_returning_fake_empty_success(string payload)
    {
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new EraEquipmentConfiguration { Version = 1, Payload = payload });

        var response = await Service().GetManifestAsync(new GetEraEquipmentManifestRequest(), CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.ServiceUnavailable);
        response.ErrorCode.ShouldBe("era.equipment_config_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_report_missing_catalog_record()
    {
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);

        var response = await Service().GetManifestAsync(new GetEraEquipmentManifestRequest(), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_config_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_unpublished_storage_version_even_when_payload_claims_a_valid_version()
    {
        var configuration = Configuration();
        configuration.Version = 0;
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(configuration);

        var response = await Service().GetManifestAsync(new GetEraEquipmentManifestRequest(), CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_config_unavailable");
    }

    [Fact]
    public async Task Should_return_empty_character_without_creating_assets_or_using_character_profile_version()
    {
        _snapshot.EquipmentVersion = 0;
        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.EquipmentVersion.ShouldBe(0);
        response.Data.TotalInventoryCapacity.ShouldBe(120);
        response.Data.TotalInventoryCount.ShouldBe(0);
        response.Data.InventoryItems.ShouldBeEmpty();
        response.Data.EquippedSlots.Count.ShouldBe(9);
        response.Data.EquippedSlots.All(slot => slot.Equipment == null).ShouldBeTrue();
        await _players.DidNotReceive().LockCharacterAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_return_saved_values_and_distinct_ids_despite_matching_names()
    {
        var first = Entry();
        var second = Entry();
        _snapshot.Entries.AddRange(new[] { first, second });

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.Data!.InventoryItems.Count.ShouldBe(2);
        response.Data.InventoryItems.Select(item => item.Id).Distinct().Count().ShouldBe(2);
        response.Data.InventoryItems.All(item => item.Definition!.Name == "Same display name").ShouldBeTrue();
        response.Data.InventoryItems.All(item => item.ArmorAttributes.Strength == 120).ShouldBeTrue();
        response.Data.InventoryItems.All(item => item.GenerationRuleVersion == 42 && item.DefinitionVersion == 2 && item.Version == 5).ShouldBeTrue();
        response.Data.ManifestVersion.ShouldBe(7);
        first.Armor!.Strength.ShouldBe(120);
    }

    [Fact]
    public async Task Should_filter_only_inventory_candidates_and_keep_full_counts_and_equipped_snapshot()
    {
        var inventoryHead = Entry();
        var equippedHead = Entry(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped);
        _snapshot.Entries.AddRange(new[] { inventoryHead, equippedHead, Entry(EraEquipmentSlot.Body) });
        var service = Service();

        var filtered = await service.GetInventoryAsync(new GetEraCharacterInventoryRequest
        {
            CharacterId = _character.Id, Slot = EraEquipmentSlot.Head
        }, CancellationToken.None);
        var complete = await service.GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        filtered.Data!.EquipmentVersion.ShouldBe(complete.Data!.EquipmentVersion);
        filtered.Data.TotalInventoryCount.ShouldBe(2);
        filtered.Data.InventoryItems.Select(item => item.Id).ShouldBe(new[] { inventoryHead.Equipment.Id });
        filtered.Data.EquippedSlots.Single(slot => slot.Slot == EraEquipmentSlot.Head).Equipment!.Id.ShouldBe(equippedHead.Equipment.Id);
        complete.Data.InventoryItems.Count.ShouldBe(2);
    }

    [Fact]
    public async Task Should_allow_one_slot_to_use_all_120_inventory_spaces_and_exclude_equipped_items()
    {
        for (var index = 0; index < 120; index++) _snapshot.Entries.Add(Entry());
        _snapshot.Entries.Add(Entry(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped));

        var response = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.TotalInventoryCount.ShouldBe(120);
        response.Data.TotalInventoryCapacity.ShouldBe(120);
        response.Data.InventoryItems.Count.ShouldBe(120);
        response.Data.EquippedSlots.Count(slot => slot.Equipment != null).ShouldBe(1);
    }

    [Fact]
    public async Task Should_report_overfull_inventory_instead_of_hiding_the_121st_item()
    {
        for (var index = 0; index < 121; index++) _snapshot.Entries.Add(Entry());

        var response = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_share_120_inventory_spaces_across_slots_and_exclude_six_equipped_items()
    {
        foreach (var slot in EraEquipmentRules.ArmorSlots)
        {
            for (var index = 0; index < 20; index++) _snapshot.Entries.Add(Entry(slot));
            _snapshot.Entries.Add(Entry(slot, EraEquipmentLocation.Equipped));
        }

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.TotalInventoryCount.ShouldBe(120);
        response.Data.TotalInventoryCapacity.ShouldBe(120);
        response.Data.EquippedSlots.Where(slot => EraEquipmentRules.ArmorSlots.Contains(slot.Slot)).All(slot => slot.Equipment != null).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_keep_missing_definition_and_catalog_assets_visible()
    {
        var entry = Entry(EraEquipmentSlot.Body);
        _snapshot.Entries.Add(entry);
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);

        var response = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest
        {
            CharacterId = _character.Id
        }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.ManifestVersion.ShouldBeNull();
        response.Data.InventoryItems.Single().Id.ShouldBe(entry.Equipment.Id);
        response.Data.InventoryItems.Single().Definition.ShouldBeNull();
        response.Data.InventoryItems.Single().ArmorAttributes.Strength.ShouldBe(120);
    }

    [Fact]
    public async Task Should_allow_reading_disabled_character_without_claiming_it_can_be_used()
    {
        _character.IsDisabled = true;
        _snapshot.IsCharacterDisabled = true;

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.IsCharacterDisabled.ShouldBeTrue();
    }

    [Fact]
    public async Task Should_reject_other_players_character_before_loading_equipment()
    {
        _character.PlayerId = Guid.NewGuid();

        await Should.ThrowAsync<EraPlayerAccessException>(() => Service().GetEquipmentAsync(
            new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None));

        await _data.DidNotReceive().GetCharacterSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_not_treat_disappeared_character_as_empty_inventory()
    {
        _data.GetCharacterSnapshotAsync(_character.Id, _player.Id, Arg.Any<CancellationToken>()).Returns((EraEquipmentReadSnapshot?)null);

        await Should.ThrowAsync<EraPlayerAccessException>(() => Service().GetInventoryAsync(
            new GetEraCharacterInventoryRequest { CharacterId = _character.Id }, CancellationToken.None));
    }

    [Fact]
    public async Task Should_report_missing_armor_data_instead_of_dropping_the_instance()
    {
        _snapshot.Entries.Add(new EraEquipmentReadEntry(Entry().Equipment, null));

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_not_show_duplicate_instance_as_multiple_inventory_items()
    {
        var entry = Entry();
        _snapshot.Entries.AddRange(new[] { entry, entry });

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_duplicate_equipped_slot()
    {
        _snapshot.Entries.Add(Entry(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped));
        _snapshot.Entries.Add(Entry(EraEquipmentSlot.Head, EraEquipmentLocation.Equipped));

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_inventory_instance_with_equipped_slot()
    {
        var entry = Entry();
        entry.Equipment.EquippedSlot = EraEquipmentSlot.Head;
        _snapshot.Entries.Add(entry);

        var response = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_mismatched_instance_owner()
    {
        var entry = Entry();
        entry.Equipment.OwnerPlayerId = Guid.NewGuid();
        _snapshot.Entries.Add(entry);

        var response = await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_data_unavailable");
        response.Data.ShouldBeNull();
    }

    [Fact]
    public async Task Should_reject_invalid_filter_slot()
    {
        var response = await Service().GetInventoryAsync(new GetEraCharacterInventoryRequest
        {
            CharacterId = _character.Id, Slot = (EraEquipmentSlot)0
        }, CancellationToken.None);

        response.ErrorCode.ShouldBe("era.equipment_invalid_slot");
        await _data.DidNotReceive().GetCharacterSnapshotAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Should_reject_duplicate_and_empty_definition_ids()
    {
        var manifest = new EraEquipmentManifestDto
        {
            Version = 1,
            Armors = new EraArmorCatalogDto { Templates = new List<EraArmorTemplateDto> { Definition(), Definition() } }
        };
        Should.Throw<ArgumentException>(() => EraEquipmentConfigurationValidator.Validate(manifest));

        manifest.Armors.Templates[1].Id = Guid.NewGuid();
        Should.NotThrow(() => EraEquipmentConfigurationValidator.Validate(manifest));
        manifest.Armors.Templates[1].Id = Guid.Empty;
        Should.Throw<ArgumentException>(() => EraEquipmentConfigurationValidator.Validate(manifest));
    }

    [Fact]
    public void Should_allow_any_valid_armor_compatible_slots_without_a_stored_inventory_classification()
    {
        var definition = Definition();
        definition.CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Body };
        Should.NotThrow(() => EraEquipmentConfigurationValidator.Validate(new EraEquipmentManifestDto
        {
            Version = 1, Armors = new EraArmorCatalogDto { Templates = new List<EraArmorTemplateDto> { definition } }
        }));
    }

    [Fact]
    public async Task Should_filter_from_current_template_compatibility_and_exclude_missing_definitions()
    {
        var item = Entry();
        _snapshot.Entries.Add(item);
        var configuration = Configuration();
        var manifest = JsonSerializer.Deserialize<EraEquipmentManifestDto>(configuration.Payload)!;
        manifest.Armors.Templates.Single().CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Head, EraEquipmentSlot.Body };
        configuration.Payload = JsonSerializer.Serialize(manifest);
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns(configuration);
        var request = new GetEraCharacterInventoryRequest { CharacterId = _character.Id, Slot = EraEquipmentSlot.Body };
        var compatible = await Service().GetInventoryAsync(request, CancellationToken.None);
        compatible.Data!.InventoryItems.Single().Id.ShouldBe(item.Equipment.Id);
        manifest.Armors.Templates.Single().CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Head };
        configuration.Payload = JsonSerializer.Serialize(manifest);
        (await Service().GetInventoryAsync(request, CancellationToken.None)).Data!.InventoryItems.ShouldBeEmpty();
        _data.GetConfigurationAsync(Arg.Any<CancellationToken>()).Returns((EraEquipmentConfiguration?)null);
        var missing = await Service().GetInventoryAsync(request, CancellationToken.None);
        missing.Data!.InventoryItems.ShouldBeEmpty();
        missing.Data.TotalInventoryCount.ShouldBe(1);
        (await Service().GetEquipmentAsync(new GetEraCharacterEquipmentRequest { CharacterId = _character.Id }, CancellationToken.None))
            .Data!.InventoryItems.Single().Id.ShouldBe(item.Equipment.Id);
    }
}
