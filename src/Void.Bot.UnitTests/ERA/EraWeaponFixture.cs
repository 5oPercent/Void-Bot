using System.Net;
using System.Text.Json;
using System.Text.Json.Nodes;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Equipment.Models;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWeaponFixture
{
    internal static EraWeaponCatalogDto Catalog()
    {
        var catalog = new EraWeaponCatalogDto();
        catalog.StatMappings.Add(new EraWeaponStatMappingDto
        {
            Id = Guid.Parse("40000000-0000-0000-0000-000000000001"), Name = "Test rifle effects",
            SpreadDegreesAtZero = 3, SpreadDegreesAtHundred = 1, RecoilDegreesAtZero = 2, RecoilDegreesAtHundred = .5,
            ReadySecondsAtZero = 1, ReadySecondsAtHundred = .35, AimSecondsAtZero = .5, AimSecondsAtHundred = .15
        });
        var configuration = new EraWeaponSlotConfigurationDto { Id = Guid.NewGuid(), Name = "Rifle slots" };
        catalog.SlotConfigurations.Add(configuration);
        var template = new EraWeaponTemplateDto
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Test rifle", PresentationKey = "scifi_rifle",
            StatMappingId = catalog.StatMappings[0].Id, SlotConfigurationId = configuration.Id,
            CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Weapon1, EraEquipmentSlot.Weapon2, EraEquipmentSlot.Weapon3 },
            BaseStats = new EraWeaponStatsDto
            {
                Damage = 50, Frequency = 5, LoadedCapacity = 30, ReserveCapacity = 120, ReloadSpeed = 1,
                Range = 30, Accuracy = 50, Stability = 50, Handling = 50
            },
            Behavior = new EraWeaponBehaviorDto
            {
                FireMode = EraWeaponFireMode.Automatic, HitMode = EraWeaponHitMode.Hitscan, ReloadMode = EraWeaponReloadMode.Magazine,
                BaseReloadSeconds = 2.4
            }
        };
        foreach (var slot in Enumerable.Range(1, 5))
        {
            var typeId = Guid.Parse($"50000000-0000-0000-0000-{slot:000000000000}");
            catalog.SlotTypes.Add(new EraWeaponSlotTypeDto { Id = typeId, Name = $"Type {slot}" });
            var definition = new EraWeaponConfigurationSlotDto
            {
                Id = Guid.Parse($"60000000-0000-0000-0000-{slot:000000000000}"), Name = $"Slot {slot}",
                SlotTypeId = typeId, CandidateCount = 2, DisplayOrder = slot
            };
            for (var index = 0; index < 3; index++)
            {
                var id = Guid.Parse($"30000000-0000-0000-{(int)slot:0000}-{index + 1:000000000000}");
                catalog.Modifications.Add(new EraWeaponModDefinitionDto
                {
                    Id = id, Name = $"Test {slot} {index}", SlotTypeId = typeId,
                    Modifiers = new List<EraWeaponModifierDto>
                    {
                        new EraWeaponModifierDto { Stat = EraWeaponStat.Handling, Operation = EraWeaponModifierOperation.Flat, Value = -index }
                    }
                });
                definition.Pool.Add(new EraWeaponModPoolEntryDto { ModDefinitionId = id, Weight = index + 1 });
            }
            configuration.Slots.Add(definition);
        }
        catalog.Templates.Add(template);
        return catalog;
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(4)]
    [InlineData(17)]
    public void Should_reject_unfulfillable_or_out_of_bound_candidate_counts(int count)
    {
        var catalog = Catalog();
        catalog.SlotConfigurations[0].Slots[0].CandidateCount = count;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_require_guid_definition_ids_and_preserve_guid_references_in_json()
    {
        var catalog = Catalog();
        var restored = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(catalog))!;
        restored.Templates[0].Id.ShouldBe(catalog.Templates[0].Id);
        restored.SlotConfigurations[0].Slots[0].Pool[0].ModDefinitionId.ShouldBe(catalog.Modifications[0].Id);
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(restored));
        restored.Templates[0].Id = Guid.Empty;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(restored));
        restored = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(catalog))!;
        restored.Modifications[0].Id = Guid.Empty;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(restored));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponTemplateDto>("{\"Id\":\"test.rifle\"}"));
    }

    [Theory]
    [InlineData("BaseSpreadDegrees")]
    [InlineData("BaseRecoilDegrees")]
    [InlineData("BaseReadySeconds")]
    [InlineData("BaseAimSeconds")]
    public void Should_reject_obsolete_score_effect_baselines_and_keep_reload_duration(string property)
    {
        var catalog = Catalog();
        var behavior = catalog.Templates[0].Behavior;
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        var restored = JsonSerializer.Deserialize<EraWeaponBehaviorDto>(JsonSerializer.Serialize(behavior))!;
        restored.BaseReloadSeconds.ShouldBe(2.4);
        var payload = JsonNode.Parse(JsonSerializer.Serialize(behavior))!;
        payload[property] = 1;
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponBehaviorDto>(payload.ToJsonString()));
    }

    [Theory]
    [InlineData(double.NaN, 1)]
    [InlineData(3, double.PositiveInfinity)]
    [InlineData(181, 1)]
    [InlineData(1, 3)]
    [InlineData(3, -1)]
    public void Should_reject_invalid_score_effect_endpoints(double zero, double hundred)
    {
        var catalog = Catalog();
        catalog.StatMappings[0].SpreadDegreesAtZero = zero;
        catalog.StatMappings[0].SpreadDegreesAtHundred = hundred;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_require_referenced_stat_mapping_and_positive_action_duration()
    {
        var catalog = Catalog();
        catalog.StatMappings[0].ReadySecondsAtHundred = 0;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog = Catalog();
        catalog.StatMappings.Clear();
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Theory]
    [InlineData("Templates")]
    [InlineData("Templates.0")]
    [InlineData("Modifications")]
    [InlineData("Modifications.0")]
    [InlineData("Modifications.0.Modifiers")]
    [InlineData("Modifications.0.Modifiers.0")]
    [InlineData("Templates.0.CompatibleSlots")]
    [InlineData("Templates.0.Slots")]
    [InlineData("Templates.0.Slots.0")]
    [InlineData("Templates.0.Slots.0.Pool")]
    [InlineData("Templates.0.Slots.0.Pool.0")]
    [InlineData("Templates.0.BaseStats")]
    [InlineData("Templates.0.Behavior")]
    public void Should_reject_null_json_members_before_enumerating_or_reading_them(string path)
    {
        var document = JsonNode.Parse(JsonSerializer.Serialize(Catalog()))!;
        var segments = path.Split('.');
        var parent = document;
        foreach (var segment in segments[..^1])
            parent = parent is JsonArray array ? array[int.Parse(segment)]! : parent[segment]!;
        if (parent is JsonArray items) items[int.Parse(segments[^1])] = null;
        else parent[segments[^1]] = null;

        var catalog = JsonSerializer.Deserialize<EraWeaponCatalogDto>(document.ToJsonString())!;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_reject_null_configuration_roots()
    {
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(null!));
        Should.Throw<ArgumentException>(() => EraEquipmentConfigurationValidator.Validate(null!));
        Should.Throw<ArgumentException>(() => EraRewardConfigurationValidator.Validate(null!));
    }

    [Fact]
    public void Should_reject_missing_candidate_count_and_unrecognized_instance_stat_inputs()
    {
        var catalog = Catalog();
        var payload = JsonSerializer.Serialize(catalog).Replace("\"CandidateCount\":2,", string.Empty);
        var missing = JsonSerializer.Deserialize<EraWeaponCatalogDto>(payload)!;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(missing));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<SelectEraWeaponModificationCommand>("{\"Damage\":9999}"));
    }

    [Fact]
    public void Should_map_template_stats_without_mutating_the_instance_contract()
    {
        var configuration = new AutoMapper.MapperConfiguration(mapping => mapping.AddProfile<Void.Bot.Core.Mappings.EraWeaponMapping>());
        configuration.AssertConfigurationIsValid();
        var mapper = configuration.CreateMapper();
        var entity = new EraWeaponTemplate { Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Damage = 55, LoadedCapacity = 30, Frequency = 5 };
        var dto = mapper.Map<EraWeaponTemplateDto>(entity);
        dto.BaseStats.Damage.ShouldBe(55);
        dto.BaseStats.LoadedCapacity.ShouldBe(30);
        mapper.Map(new EraWeaponStatsDto { Damage = 60, LoadedCapacity = 40 }, entity);
        entity.Id.ShouldBe(Guid.Parse("20000000-0000-0000-0000-000000000001"));
        entity.Damage.ShouldBe(60);
    }

    [Fact]
    public void Should_reject_duplicate_pool_entries_and_unsupported_modifiers()
    {
        var catalog = Catalog();
        catalog.SlotConfigurations[0].Slots[0].Pool[1].ModDefinitionId = catalog.SlotConfigurations[0].Slots[0].Pool[0].ModDefinitionId;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog = Catalog();
        catalog.Modifications[0].Modifiers[0].Stat = EraWeaponStat.ChargeTime;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_generate_exact_distinct_candidates_per_slot_and_only_select_generated_candidates()
    {
        var catalog = Catalog();
        catalog.SlotConfigurations[0].Slots[0].CandidateCount = 1;
        catalog.SlotConfigurations[0].Slots[1].CandidateCount = 3;
        EraWeaponConfigurationValidator.Validate(catalog);
        var instance = EraWeaponGenerator.Generate(catalog.SlotConfigurations[0], _ => 0);
        instance.Slots.Select(slot => slot.Candidates.Count).ShouldBe(new[] { 1, 3, 2, 2, 2 });
        instance.Slots.All(slot => slot.Candidates.Select(candidate => candidate.ModDefinitionId).Distinct().Count() == slot.Candidates.Count).ShouldBeTrue();
        instance.Slots.All(slot => slot.Candidates.Any(candidate => candidate.CandidateId == slot.SelectedCandidateId)).ShouldBeTrue();
        EraEquipmentItemRules.ValidWeapon(instance).ShouldBeTrue();
    }

    [Fact]
    public void Should_respect_weight_interval_boundaries_and_remove_each_drawn_entry()
    {
        var catalog = Catalog();
        var template = catalog.Templates[0];
        var bounds = new List<int>();
        var instance = EraWeaponGenerator.Generate(catalog.SlotConfigurations[0], bound => { bounds.Add(bound); return bound - 1; });
        bounds.ShouldBe(new[] { 6, 3, 6, 3, 6, 3, 6, 3, 6, 3 });
        foreach (var slot in instance.Slots)
        {
            var pool = catalog.SlotConfigurations[0].Slots.Single(value => value.Id == slot.SlotId).Pool;
            slot.Candidates[0].ModDefinitionId.ShouldBe(pool[2].ModDefinitionId);
            slot.Candidates[1].ModDefinitionId.ShouldBe(pool[1].ModDefinitionId);
        }
    }

    [Fact]
    public void Should_reject_selected_candidate_from_another_slot()
    {
        var instance = EraWeaponGenerator.Generate(Catalog().SlotConfigurations[0], _ => 0);
        instance.Slots[0].SelectedCandidateId = instance.Slots[1].Candidates[0].CandidateId;
        EraEquipmentItemRules.ValidWeapon(instance).ShouldBeFalse();
    }

    [Fact]
    public void Should_follow_template_stats_without_changing_old_candidates_when_generation_pool_changes()
    {
        var catalog = Catalog();
        var template = catalog.Templates[0];
        var instance = EraWeaponGenerator.Generate(catalog.SlotConfigurations[0], _ => 0);
        var before = JsonSerializer.Serialize(instance);
        var character = Guid.NewGuid();
        var player = Guid.NewGuid();
        var equipment = new EraEquipmentInstance
        {
            Id = Guid.NewGuid(), DefinitionId = template.Id, DefinitionVersion = 2, EquipmentType = EraEquipmentType.Weapon,
            Grade = EraEquipmentGrade.Common, OwnerPlayerId = player, CharacterId = character, Location = EraEquipmentLocation.Inventory,
            Version = 3, CreatedAt = DateTimeOffset.UtcNow
        };
        template.BaseStats.Damage = 55;
        foreach (var slot in catalog.SlotConfigurations[0].Slots) { slot.Pool.RemoveAt(0); slot.CandidateCount = 1; }
        EraWeaponConfigurationValidator.Validate(catalog);
        var snapshot = new EraEquipmentReadSnapshot
        {
            CharacterId = character, PlayerId = player, EquipmentVersion = 4,
            Entries = new List<EraEquipmentReadEntry> { new EraEquipmentReadEntry(equipment, null, instance) }
        };
        var state = EraEquipmentService.CreateState(snapshot, new EraEquipmentManifestDto { Version = 9, Weapons = catalog }, null);
        state.ShouldNotBeNull();
        state.InventoryItems.Single().WeaponTemplate!.BaseStats.Damage.ShouldBe(55);
        state.InventoryItems.Single().Version.ShouldBe(3);
        state.InventoryItems.Single().DefinitionVersion.ShouldBe(2);
        JsonSerializer.Serialize(state.InventoryItems.Single().Weapon).ShouldBe(before);
        instance.Slots.All(slot => slot.Candidates.Count == 2).ShouldBeTrue();
    }

    [Fact]
    public async Task Should_publish_once_and_protect_references_without_rewriting_instances()
    {
        var catalog = Catalog();
        var weapons = Substitute.For<IEraWeaponDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        var user = Substitute.For<ICurrentUser>();
        user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 8, Payload = "{\"SchemaVersion\":1,\"Definitions\":[]}" };
        weapons.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        weapons.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(catalog);
        weapons.GetReferencedWeaponsAsync(Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance>());
        weapons.GetReferencedCandidatesAsync(Arg.Any<CancellationToken>()).Returns(new List<EraWeaponModCandidate>());
        weapons.GetReferencedSlotsAsync(Arg.Any<CancellationToken>()).Returns(new List<EraWeaponModSelection>());
        weapons.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(new List<string>());
        var service = new EraWeaponService(weapons, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        catalog.Templates[0].BaseStats.Damage = 55;
        var response = await service.SaveTemplateAsync(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 8, Template = catalog.Templates[0]
        }, CancellationToken.None);
        response.Code.ShouldBe(HttpStatusCode.OK);
        publication.Version.ShouldBe(9);
        publication.LastModifiedBy.ShouldBe(user.Id);
        await weapons.Received(1).ReplaceCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>());
        await Should.ThrowAsync<EraWeaponException>(() => service.SaveTemplateAsync(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 8, Template = catalog.Templates[0]
        }, CancellationToken.None));
        await weapons.Received(1).ReplaceCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_allow_switching_an_old_candidate_removed_from_the_current_template_pool()
    {
        var catalog = Catalog();
        var template = catalog.Templates[0];
        var instance = EraWeaponGenerator.Generate(catalog.SlotConfigurations[0], _ => 0);
        var chosen = instance.Slots[0].Candidates[1].CandidateId;
        foreach (var slot in catalog.SlotConfigurations[0].Slots) { slot.Pool.RemoveAt(1); slot.CandidateCount = 1; }
        var character = new EraCharacter { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), EquipmentVersion = 3 };
        var item = new EraEquipmentInstance
        {
            Id = Guid.NewGuid(), DefinitionId = template.Id, DefinitionVersion = 2, EquipmentType = EraEquipmentType.Weapon,
            Grade = EraEquipmentGrade.Common, CharacterId = character.Id, OwnerPlayerId = character.PlayerId,
            Location = EraEquipmentLocation.Inventory, Version = 5, CreatedAt = DateTimeOffset.UtcNow
        };
        var snapshot = new EraEquipmentReadSnapshot
        {
            CharacterId = character.Id, PlayerId = character.PlayerId, EquipmentVersion = 3,
            Entries = new List<EraEquipmentReadEntry> { new EraEquipmentReadEntry(item, null, instance) }
        };
        var data = Substitute.For<IEraEquipmentDataProvider>();
        var weapons = Substitute.For<IEraWeaponDataProvider>();
        var context = Substitute.For<IEraCharacterContext>();
        context.RequireOwnedAsync(character.Id, true, Arg.Any<CancellationToken>()).Returns(character);
        data.LockConfigurationAsync(Arg.Any<CancellationToken>()).Returns(new EraEquipmentConfiguration
        {
            Version = 8, Payload = "{\"SchemaVersion\":1,\"Definitions\":[]}"
        });
        data.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(catalog);
        data.LockCharacterEquipmentAsync(character, Arg.Any<CancellationToken>()).Returns(snapshot);
        var command = new SelectEraWeaponModificationCommand
        {
            CharacterId = character.Id, EquipmentId = item.Id, RequestId = Guid.NewGuid(),
            SlotId = instance.Slots[0].SlotId, CandidateId = chosen, ExpectedManifestVersion = 8, ExpectedEquipmentVersion = 3, ExpectedItemVersion = 5
        };
        var response = await new EraEquipmentService(data, context, Substitute.For<ILocalizationService>(), weapons)
            .SelectWeaponModificationAsync(command, CancellationToken.None);
        response.Code.ShouldBe(HttpStatusCode.OK);
        response.Data!.State.EquipmentVersion.ShouldBe(4);
        response.Data.State.InventoryItems.Single().Weapon!.Slots[0].SelectedCandidateId.ShouldBe(chosen);
        await weapons.Received(1).UpdateSelectionAsync(item.Id, command.SlotId, Arg.Any<Guid>(), chosen, 5, Arg.Any<CancellationToken>());
        instance.Slots[0].Candidates.Count.ShouldBe(2);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(12)]
    public void Should_generate_dynamic_slot_counts_and_allow_repeated_types(int count)
    {
        var catalog = Catalog();
        var configuration = catalog.SlotConfigurations[0];
        var first = configuration.Slots[0];
        configuration.Slots = Enumerable.Range(0, count).Select(index => new EraWeaponConfigurationSlotDto
        {
            Id = Guid.NewGuid(), Name = $"Optic {index}", SlotTypeId = first.SlotTypeId, DisplayOrder = index,
            CandidateCount = first.CandidateCount, Pool = first.Pool.ToList()
        }).ToList();
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        var instance = EraWeaponGenerator.Generate(configuration, _ => 0);
        instance.Slots.Count.ShouldBe(count);
        EraEquipmentItemRules.ValidWeapon(instance).ShouldBeTrue();
        instance.Slots.Select(slot => slot.SlotId).Distinct().Count().ShouldBe(count);
        var snapshot = JsonSerializer.Serialize(instance);
        configuration.Slots.Clear();
        JsonSerializer.Serialize(instance).ShouldBe(snapshot);
        EraEquipmentItemRules.ValidWeapon(instance).ShouldBeTrue();
    }

    [Fact]
    public void Should_validate_every_template_sharing_a_scheme_and_reject_duplicate_slot_ids()
    {
        var catalog = Catalog();
        catalog.Modifications[0].Modifiers[0].Stat = EraWeaponStat.ProjectileSpeed;
        catalog.Templates[0].Behavior.HitMode = EraWeaponHitMode.Projectile;
        catalog.Templates[0].BaseStats.ProjectileSpeed = 100;
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        var other = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(catalog.Templates[0]))!;
        other.Id = Guid.NewGuid(); other.Behavior.HitMode = EraWeaponHitMode.Hitscan; other.BaseStats.ProjectileSpeed = null;
        catalog.Templates.Add(other);
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog.Templates.Remove(other);
        var copy = JsonSerializer.Deserialize<EraWeaponSlotConfigurationDto>(JsonSerializer.Serialize(catalog.SlotConfigurations[0]))!;
        copy.Id = Guid.NewGuid(); catalog.SlotConfigurations.Add(copy);
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        foreach (var slot in copy.Slots) slot.Id = Guid.NewGuid();
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
    }
}
