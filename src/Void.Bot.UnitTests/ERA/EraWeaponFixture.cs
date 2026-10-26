using Void.Bot.Core.Services.ERA.Reward.Models;
using AutoMapper;
using Void.Bot.Core.Mappings;
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
    [InlineData("StatMappings")]
    [InlineData("StatMappings.0")]
    [InlineData("SlotTypes")]
    [InlineData("SlotTypes.0")]
    [InlineData("SlotConfigurations")]
    [InlineData("SlotConfigurations.0")]
    [InlineData("Templates.0.CompatibleSlots")]
    [InlineData("SlotConfigurations.0.Slots")]
    [InlineData("SlotConfigurations.0.Slots.0")]
    [InlineData("SlotConfigurations.0.Slots.0.Pool")]
    [InlineData("SlotConfigurations.0.Slots.0.Pool.0")]
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
        var configuration = new MapperConfiguration(mapping => mapping.AddProfile<EraWeaponMapping>());
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
            Version = 8, Payload = "{\"SchemaVersion\":2}"
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

    [Fact]
    public void Should_default_missing_dto_ids_and_reject_explicit_empty_ids_in_validators()
    {
        var generated = new[]
        {
            JsonSerializer.Deserialize<EraWeaponTemplateDto>("{}")!.Id,
            JsonSerializer.Deserialize<EraWeaponModDefinitionDto>("{}")!.Id,
            JsonSerializer.Deserialize<EraWeaponStatMappingDto>("{}")!.Id,
            JsonSerializer.Deserialize<EraWeaponSlotTypeDto>("{}")!.Id,
            JsonSerializer.Deserialize<EraWeaponSlotConfigurationDto>("{}")!.Id,
            JsonSerializer.Deserialize<EraWeaponConfigurationSlotDto>("{}")!.Id
        };
        generated.ShouldAllBe(id => id != Guid.Empty);
        generated.Distinct().Count().ShouldBe(generated.Length);
        var type = JsonSerializer.Deserialize<EraWeaponSlotTypeDto>("{\"Id\":\"00000000-0000-0000-0000-000000000000\"}")!;
        var results = new[]
        {
            new SaveEraWeaponTemplateCommandValidator().Validate(new SaveEraWeaponTemplateCommand
            {
                ExpectedManifestVersion = 7, Template = new EraWeaponTemplateDto { Id = Guid.Empty }
            }),
            new SaveEraWeaponModificationCommandValidator().Validate(new SaveEraWeaponModificationCommand
            {
                ExpectedManifestVersion = 7, Modification = new EraWeaponModDefinitionDto { Id = Guid.Empty }
            }),
            new SaveEraWeaponStatMappingCommandValidator().Validate(new SaveEraWeaponStatMappingCommand
            {
                ExpectedManifestVersion = 7, StatMapping = new EraWeaponStatMappingDto { Id = Guid.Empty }
            }),
            new SaveEraWeaponSlotTypeCommandValidator().Validate(new SaveEraWeaponSlotTypeCommand
            {
                ExpectedManifestVersion = 7, SlotType = type
            }),
            new SaveEraWeaponSlotConfigurationCommandValidator().Validate(new SaveEraWeaponSlotConfigurationCommand
            {
                ExpectedManifestVersion = 7, SlotConfiguration = new EraWeaponSlotConfigurationDto { Id = Guid.Empty }
            }),
            new SaveEraWeaponSlotConfigurationCommandValidator().Validate(new SaveEraWeaponSlotConfigurationCommand
            {
                ExpectedManifestVersion = 7, SlotConfiguration = new EraWeaponSlotConfigurationDto
                {
                    Slots = new List<EraWeaponConfigurationSlotDto> { new EraWeaponConfigurationSlotDto { Id = Guid.Empty } }
                }
            })
        };
        results.ShouldAllBe(result => !result.IsValid);
        type.Id.ShouldBe(Guid.Empty);
        new SaveEraWeaponTemplateCommandValidator().Validate(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = null!
        }).IsValid.ShouldBeFalse();
        new SaveEraWeaponSlotConfigurationCommandValidator().Validate(new SaveEraWeaponSlotConfigurationCommand
        {
            ExpectedManifestVersion = 7, SlotConfiguration = new EraWeaponSlotConfigurationDto { Slots = null! }
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public async Task Should_preserve_dto_generated_ids_when_creating_and_updating_configurations()
    {
        var current = new EraWeaponCatalogDto();
        var weapons = Substitute.For<IEraWeaponDataProvider>();
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        weapons.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        weapons.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(current));
        weapons.GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance>());
        weapons.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(new List<string>());
        weapons.SaveCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            current = call.ArgAt<EraWeaponCatalogDto>(1); return Task.CompletedTask;
        });
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraArmorCatalogDto());
        var service = new EraWeaponService(weapons, equipment, user,
            Substitute.For<ILocalizationService>(), TimeProvider.System);
        var first = await service.SaveSlotTypeAsync(new SaveEraWeaponSlotTypeCommand
        {
            ExpectedManifestVersion = 7, SlotType = new EraWeaponSlotTypeDto { Name = "Optic" }
        }, CancellationToken.None);
        var firstId = first.Data!.SavedId!.Value;
        firstId.ShouldNotBe(Guid.Empty);
        var second = await service.SaveSlotTypeAsync(new SaveEraWeaponSlotTypeCommand
        {
            ExpectedManifestVersion = 8, SlotType = new EraWeaponSlotTypeDto { Name = "Muzzle" }
        }, CancellationToken.None);
        second.Data!.SavedId.ShouldNotBe(first.Data.SavedId);
        second.Data.Configuration.SlotTypes.Count.ShouldBe(2);
        var update = await service.SaveSlotTypeAsync(new SaveEraWeaponSlotTypeCommand
        {
            ExpectedManifestVersion = 9, SlotType = new EraWeaponSlotTypeDto { Id = firstId, Name = "Main optic" }
        }, CancellationToken.None);
        update.Data!.SavedId.ShouldBe(firstId);
        update.Data.Configuration.SlotTypes.Count.ShouldBe(2);
        var modification = await service.SaveModificationAsync(new SaveEraWeaponModificationCommand
        {
            ExpectedManifestVersion = 10,
            Modification = new EraWeaponModDefinitionDto { Name = "Optic attachment", SlotTypeId = firstId }
        }, CancellationToken.None);
        var scheme = await service.SaveSlotConfigurationAsync(new SaveEraWeaponSlotConfigurationCommand
        {
            ExpectedManifestVersion = 11,
            SlotConfiguration = new EraWeaponSlotConfigurationDto
            {
                Name = "Optics",
                Slots = new List<EraWeaponConfigurationSlotDto>
                {
                    new EraWeaponConfigurationSlotDto
                    {
                        Name = "Main optic", SlotTypeId = firstId, CandidateCount = 1,
                        Pool = new List<EraWeaponModPoolEntryDto>
                        {
                            new EraWeaponModPoolEntryDto { ModDefinitionId = modification.Data!.SavedId!.Value, Weight = 1 }
                        }
                    }
                }
            }
        }, CancellationToken.None);
        var schemeId = scheme.Data!.SavedId!.Value;
        schemeId.ShouldNotBe(Guid.Empty);
        var savedScheme = scheme.Data.Configuration.SlotConfigurations.Single();
        var savedSlotId = savedScheme.Slots.Single().Id;
        savedSlotId.ShouldNotBe(Guid.Empty);
        var edited = JsonSerializer.Deserialize<EraWeaponSlotConfigurationDto>(JsonSerializer.Serialize(savedScheme))!;
        edited.Slots[0].Name = "Renamed optic";
        var extra = new EraWeaponConfigurationSlotDto
        {
            Name = "Secondary optic", SlotTypeId = firstId, CandidateCount = 1, DisplayOrder = 1,
            Pool = edited.Slots[0].Pool.ToList()
        };
        edited.Slots.Add(extra);
        var updatedScheme = await service.SaveSlotConfigurationAsync(new SaveEraWeaponSlotConfigurationCommand
        {
            ExpectedManifestVersion = 12, SlotConfiguration = edited
        }, CancellationToken.None);
        updatedScheme.Data!.Configuration.SlotConfigurations.Single().Slots[0].Id.ShouldBe(savedSlotId);
        extra.Id.ShouldNotBe(Guid.Empty); extra.Id.ShouldNotBe(savedSlotId);
        var invalidScheme = JsonSerializer.Deserialize<EraWeaponSlotConfigurationDto>(JsonSerializer.Serialize(edited))!;
        invalidScheme.Slots[1].Id = Guid.Empty;
        var invalidSlot = await Should.ThrowAsync<EraWeaponException>(() => service.SaveSlotConfigurationAsync(new SaveEraWeaponSlotConfigurationCommand
        {
            ExpectedManifestVersion = 13, SlotConfiguration = invalidScheme
        }, CancellationToken.None));
        invalidSlot.Code.ShouldBe(HttpStatusCode.BadRequest);
        publication.Version.ShouldBe(13);
    }
    [Fact]
    public async Task Should_skip_asset_queries_for_display_edits_and_disable_without_mutating_the_previous_catalog()
    {
        var current = Catalog();
        var data = Substitute.For<IEraWeaponDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraArmorCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(current));
        data.SaveCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            current = call.ArgAt<EraWeaponCatalogDto>(1); return Task.CompletedTask;
        });
        var service = new EraWeaponService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var edited = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(current.Templates[0]))!;
        edited.Name = "Renamed";
        var saved = await service.SaveTemplateAsync(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = edited
        }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        saved.Data!.Version.ShouldBe(8);
        publication.Version.ShouldBe(8);
        publication.LastModifiedBy.ShouldBe(user.Id);
        var conflict = await Should.ThrowAsync<EraWeaponException>(() => service.SaveTemplateAsync(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = edited
        }, CancellationToken.None));
        conflict.Code.ShouldBe(HttpStatusCode.Conflict);
        await data.Received(1).SaveCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>());
        var beforeDisable = current;
        var disabled = await service.DisableTemplateAsync(new DisableEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 8, Id = edited.Id
        }, CancellationToken.None);
        disabled.Data!.Configuration.Templates[0].IsGenerationEnabled.ShouldBeFalse();
        beforeDisable.Templates[0].IsGenerationEnabled.ShouldBeTrue();
        await data.DidNotReceive().GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetPendingRewardPlansAsync(Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetRewardConfigurationPayloadAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_check_only_the_template_whose_equipped_slot_was_removed()
    {
        var current = Catalog();
        var data = Substitute.For<IEraWeaponDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraArmorCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(current);
        var templateId = current.Templates[0].Id;
        var removedSlot = current.Templates[0].CompatibleSlots[1];
        var item = new EraEquipmentInstance { Id = Guid.NewGuid(), DefinitionId = templateId, EquippedSlot = removedSlot };
        data.GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance> { item });
        data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, EraWeaponInstanceDto> { [item.Id] = new() });
        var service = new EraWeaponService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var edited = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(current.Templates[0]))!;
        edited.CompatibleSlots.Remove(removedSlot);
        var error = await Should.ThrowAsync<EraWeaponException>(() => service.SaveTemplateAsync(new SaveEraWeaponTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = edited
        }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        await data.Received(1).GetReferencedWeaponsAsync(Arg.Is<EraEquipmentReferenceChanges>(changes =>
            changes.TemplateIds.SetEquals(new[] { templateId }) && changes.ModificationIds.Count == 0 && changes.SlotTypeIds.Count == 0), Arg.Any<CancellationToken>());
        await data.DidNotReceive().SaveCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>());
        publication.Version.ShouldBe(7);
    }

    [Fact]
    public async Task Should_protect_a_pending_candidate_even_after_its_slot_was_removed_from_the_current_scheme()
    {
        var current = Catalog();
        var instance = EraWeaponGenerator.Generate(current.SlotConfigurations[0], _ => 0);
        var referencedId = instance.Slots[0].Candidates[0].ModDefinitionId;
        var proposed = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(current))!;
        proposed.SlotConfigurations[0].Slots.Clear();
        proposed.Modifications.RemoveAll(row => row.Id == referencedId);
        var plan = new EraInventoryRewardPlan { Grants = new()
        {
            new EraPlannedRewardGrant { Equipment = new()
            {
                new EraPlannedRewardEquipment { DefinitionId = current.Templates[0].Id,
                    EquipmentType = EraEquipmentType.Weapon, Weapon = instance }
            } }
        } };
        var data = Substitute.For<IEraWeaponDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraArmorCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(current);
        data.GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance>());
        data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, EraWeaponInstanceDto>());
        data.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(new List<string> { JsonSerializer.Serialize(plan) });
        var service = new EraWeaponService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var error = await Should.ThrowAsync<EraWeaponException>(() => service.PublishConfigurationAsync(new PublishEraWeaponConfigurationCommand
        {
            ExpectedManifestVersion = 7, Configuration = proposed
        }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        await data.Received(1).GetReferencedWeaponsAsync(Arg.Is<EraEquipmentReferenceChanges>(changes =>
            changes.ModificationIds.SetEquals(new[] { referencedId }) && changes.TemplateIds.Count == 0), Arg.Any<CancellationToken>());
        await data.DidNotReceive().SaveCatalogAsync(Arg.Any<EraWeaponCatalogDto>(), Arg.Any<EraWeaponCatalogDto>(), Arg.Any<CancellationToken>());
        publication.Version.ShouldBe(7);
    }

}
