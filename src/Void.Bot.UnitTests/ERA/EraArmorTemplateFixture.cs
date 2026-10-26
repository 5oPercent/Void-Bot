using Void.Bot.Core.Services.ERA.Reward.Models;
using Shouldly;
using System.Text.Json;
using NSubstitute;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Player;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Exceptions;
using System.Net;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraArmorTemplateFixture
{
    [Fact]
    public async Task Should_replay_confirmed_armor_selection_without_writing_or_rerolling()
    {
        var data = Substitute.For<IEraEquipmentDataProvider>();
        var characters = Substitute.For<IEraCharacterContext>();
        var armors = Substitute.For<IEraArmorDataProvider>();
        var character = new EraCharacter { Id = Guid.NewGuid(), PlayerId = Guid.NewGuid(), EquipmentVersion = 8 };
        characters.RequireOwnedAsync(character.Id, true, Arg.Any<CancellationToken>()).Returns(character);
        var command = new SelectEraArmorModificationCommand
        {
            CharacterId = character.Id, EquipmentId = Guid.NewGuid(), RequestId = Guid.NewGuid(),
            SlotId = Guid.NewGuid(), CandidateId = Guid.NewGuid(), ExpectedManifestVersion = 2,
            ExpectedEquipmentVersion = 3, ExpectedItemVersion = 1
        };
        var result = new EraEquipmentOperationReceiptDto
        {
            RequestId = command.RequestId, CharacterId = character.Id, Operation = EraEquipmentOperation.SelectArmorModification,
            PreviousEquipmentVersion = 3, State = new EraCharacterEquipmentStateDto { CharacterId = character.Id, EquipmentVersion = 4 }
        };
        data.GetOperationReceiptAsync(character.PlayerId, command.RequestId, Arg.Any<CancellationToken>()).Returns(new EraEquipmentOperationReceipt
        {
            PlayerId = character.PlayerId, CharacterId = character.Id, RequestId = command.RequestId,
            Operation = EraEquipmentOperation.SelectArmorModification, RequestPayload = JsonSerializer.Serialize(command), ResultPayload = JsonSerializer.Serialize(result)
        });
        var service = new EraEquipmentService(data, characters, Substitute.For<ILocalizationService>(), armorData: armors);
        var response = await service.SelectArmorModificationAsync(command, CancellationToken.None);
        response.IsReplay.ShouldBeTrue();
        response.Data!.State.EquipmentVersion.ShouldBe(4);
        await armors.DidNotReceive().UpdateSelectionAsync(Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<Guid>(), Arg.Any<long>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Should_allow_armor_without_modification_slots()
    {
        var catalog = new EraArmorCatalogDto
        {
            Templates = new() { new EraArmorTemplateDto { Name = "Plain helmet", CompatibleSlots = new() { EraEquipmentSlot.Head } } }
        };
        Should.NotThrow(() => EraArmorConfigurationValidator.Validate(catalog));
        var scheme = new EraArmorSlotConfigurationDto { Name = "Empty" };
        new EraArmorModificationGenerator().Generate(scheme).Slots.ShouldBeEmpty();
    }

    [Fact]
    public void Should_freeze_distinct_candidates_and_keep_original_six_stats_when_selecting()
    {
        var type = new EraArmorSlotTypeDto { Name = "Utility" };
        var first = new EraArmorModDefinitionDto { Name = "Strength", SlotTypeId = type.Id,
            Modifiers = new() { new EraArmorModifierDto { Stat = EraArmorAttribute.Strength, Operation = EraArmorModifierOperation.Flat, Value = 5 } } };
        var second = new EraArmorModDefinitionDto { Name = "Mobility", SlotTypeId = type.Id,
            Modifiers = new() { new EraArmorModifierDto { Stat = EraArmorAttribute.Mobility, Operation = EraArmorModifierOperation.Flat, Value = 7 } } };
        var scheme = new EraArmorSlotConfigurationDto { Name = "Utility scheme", Slots = new()
        {
            new EraArmorConfigurationSlotDto { Name = "Utility", SlotTypeId = type.Id, CandidateCount = 2,
                Pool = new() { new EraArmorModPoolEntryDto { ModDefinitionId = first.Id, Weight = 1 }, new EraArmorModPoolEntryDto { ModDefinitionId = second.Id, Weight = 1 } } }
        } };
        var catalog = new EraArmorCatalogDto { SlotTypes = new() { type }, SlotConfigurations = new() { scheme }, Modifications = new() { first, second } };
        EraArmorConfigurationValidator.Validate(catalog);
        var instance = EraArmorModificationGenerator.Generate(scheme, _ => 0);
        var slot = instance.Slots.Single();
        slot.Candidates.Select(row => row.ModDefinitionId).Distinct().Count().ShouldBe(2);
        var original = new EraArmorAttributesDto { Strength = 20, Mobility = 10 };
        EraArmorModificationRules.Evaluate(original, instance, catalog).Strength.ShouldBe(25);
        slot.SelectedCandidateId = slot.Candidates[1].CandidateId;
        var changed = EraArmorModificationRules.Evaluate(original, instance, catalog);
        changed.Strength.ShouldBe(20);
        changed.Mobility.ShouldBe(17);
        original.Strength.ShouldBe(20);
        original.Mobility.ShouldBe(10);
        scheme.Slots.Clear();
        instance.Slots.Count.ShouldBe(1);
        slot.Candidates.Count.ShouldBe(2);
    }

    [Fact]
    public void Should_reject_duplicate_types_candidates_and_unsupported_armor_attributes()
    {
        var type = new EraArmorSlotTypeDto { Name = "Core" };
        var modification = new EraArmorModDefinitionDto { Name = "Core", SlotTypeId = type.Id };
        var slot = new EraArmorConfigurationSlotDto { Name = "Core", SlotTypeId = type.Id, CandidateCount = 1,
            Pool = new() { new EraArmorModPoolEntryDto { ModDefinitionId = modification.Id, Weight = 1 } } };
        var scheme = new EraArmorSlotConfigurationDto { Name = "Core", Slots = new() { slot } };
        var catalog = new EraArmorCatalogDto { SlotTypes = new() { type }, Modifications = new() { modification }, SlotConfigurations = new() { scheme } };
        EraArmorConfigurationValidator.Validate(catalog);
        slot.Pool.Add(new EraArmorModPoolEntryDto { ModDefinitionId = modification.Id, Weight = 1 });
        Should.Throw<ArgumentException>(() => EraArmorConfigurationValidator.Validate(catalog));
        slot.Pool.RemoveAt(1);
        scheme.Slots.Add(new EraArmorConfigurationSlotDto { Name = "Duplicate type", SlotTypeId = type.Id, CandidateCount = 1, Pool = slot.Pool });
        Should.Throw<ArgumentException>(() => EraArmorConfigurationValidator.Validate(catalog));
        scheme.Slots.RemoveAt(1);
        modification.Modifiers.Add(new EraArmorModifierDto { Stat = (EraArmorAttribute)7, Operation = EraArmorModifierOperation.Flat, Value = 1 });
        Should.Throw<ArgumentException>(() => EraArmorConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public async Task Should_skip_asset_queries_for_display_edits_and_disable_without_mutating_the_previous_catalog()
    {
        var current = new EraArmorCatalogDto
        {
            Templates = new() { new EraArmorTemplateDto { Name = "Helmet", CompatibleSlots = new() { EraEquipmentSlot.Head, EraEquipmentSlot.Body } } }
        };
        var data = Substitute.For<IEraArmorDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraWeaponCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(_ => Task.FromResult(current));
        data.SaveCatalogAsync(Arg.Any<EraArmorCatalogDto>(), Arg.Any<EraArmorCatalogDto>(), Arg.Any<CancellationToken>()).Returns(call =>
        {
            current = call.ArgAt<EraArmorCatalogDto>(1); return Task.CompletedTask;
        });
        var service = new EraArmorService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var edited = JsonSerializer.Deserialize<EraArmorTemplateDto>(JsonSerializer.Serialize(current.Templates[0]))!;
        edited.Name = "Renamed";
        var saved = await service.SaveTemplateAsync(new SaveEraArmorTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = edited
        }, CancellationToken.None);
        saved.Code.ShouldBe(HttpStatusCode.OK);
        var beforeDisable = current;
        var disabled = await service.DisableTemplateAsync(new DisableEraArmorTemplateCommand
        {
            ExpectedManifestVersion = 8, Id = edited.Id
        }, CancellationToken.None);
        disabled.Data!.Configuration.Templates[0].IsGenerationEnabled.ShouldBeFalse();
        beforeDisable.Templates[0].IsGenerationEnabled.ShouldBeTrue();
        await data.DidNotReceive().GetReferencedArmorsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetPendingRewardPlansAsync(Arg.Any<CancellationToken>());
        await data.DidNotReceive().GetRewardConfigurationPayloadAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_check_only_the_template_whose_equipped_slot_was_removed()
    {
        var current = new EraArmorCatalogDto
        {
            Templates = new() { new EraArmorTemplateDto { Name = "Helmet", CompatibleSlots = new() { EraEquipmentSlot.Head, EraEquipmentSlot.Body } } }
        };
        var data = Substitute.For<IEraArmorDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraWeaponCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(current);
        var templateId = current.Templates[0].Id;
        var removedSlot = current.Templates[0].CompatibleSlots[1];
        var item = new EraEquipmentInstance { Id = Guid.NewGuid(), DefinitionId = templateId, EquippedSlot = removedSlot };
        data.GetReferencedArmorsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance> { item });
        data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, EraArmorInstanceDto> { [item.Id] = new() });
        var service = new EraArmorService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var edited = JsonSerializer.Deserialize<EraArmorTemplateDto>(JsonSerializer.Serialize(current.Templates[0]))!;
        edited.CompatibleSlots.Remove(removedSlot);
        var error = await Should.ThrowAsync<EraArmorException>(() => service.SaveTemplateAsync(new SaveEraArmorTemplateCommand
        {
            ExpectedManifestVersion = 7, Template = edited
        }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        await data.Received(1).GetReferencedArmorsAsync(Arg.Is<EraEquipmentReferenceChanges>(changes =>
            changes.TemplateIds.SetEquals(new[] { templateId }) && changes.ModificationIds.Count == 0 && changes.SlotTypeIds.Count == 0), Arg.Any<CancellationToken>());
        await data.DidNotReceive().SaveCatalogAsync(Arg.Any<EraArmorCatalogDto>(), Arg.Any<EraArmorCatalogDto>(), Arg.Any<CancellationToken>());
        publication.Version.ShouldBe(7);
    }

    [Fact]
    public async Task Should_protect_a_pending_candidate_even_after_its_slot_was_removed_from_the_current_scheme()
    {
        var type = new EraArmorSlotTypeDto { Name = "Core" };
        var modification = new EraArmorModDefinitionDto { Name = "Strength", SlotTypeId = type.Id };
        var configuration = new EraArmorSlotConfigurationDto { Name = "Core", Slots = new()
        {
            new EraArmorConfigurationSlotDto { Name = "Core", SlotTypeId = type.Id, CandidateCount = 1,
                Pool = new() { new EraArmorModPoolEntryDto { ModDefinitionId = modification.Id, Weight = 1 } } }
        } };
        var current = new EraArmorCatalogDto
        {
            SlotTypes = new() { type }, Modifications = new() { modification }, SlotConfigurations = new() { configuration },
            Templates = new() { new EraArmorTemplateDto { Name = "Helmet", SlotConfigurationId = configuration.Id,
                CompatibleSlots = new() { EraEquipmentSlot.Head } } }
        };
        var instance = EraArmorModificationGenerator.Generate(configuration, _ => 0);
        var referencedId = instance.Slots[0].Candidates[0].ModDefinitionId;
        var proposed = JsonSerializer.Deserialize<EraArmorCatalogDto>(JsonSerializer.Serialize(current))!;
        proposed.SlotConfigurations[0].Slots.Clear();
        proposed.Modifications.RemoveAll(row => row.Id == referencedId);
        var plan = new EraInventoryRewardPlan { Grants = new()
        {
            new EraPlannedRewardGrant { Equipment = new()
            {
                new EraPlannedRewardEquipment { DefinitionId = current.Templates[0].Id,
                    EquipmentType = EraEquipmentType.Armor, ArmorModifications = instance }
            } }
        } };
        var data = Substitute.For<IEraArmorDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        equipment.GetWeaponCatalogAsync(Arg.Any<CancellationToken>()).Returns(new EraWeaponCatalogDto());
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns(Guid.NewGuid());
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":2}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(publication);
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(current);
        data.GetReferencedArmorsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(new List<EraEquipmentInstance>());
        data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(new Dictionary<Guid, EraArmorInstanceDto>());
        data.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(new List<string> { JsonSerializer.Serialize(plan) });
        var service = new EraArmorService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System);
        var error = await Should.ThrowAsync<EraArmorException>(() => service.PublishConfigurationAsync(new PublishEraArmorConfigurationCommand
        {
            ExpectedManifestVersion = 7, Configuration = proposed
        }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        await data.Received(1).GetReferencedArmorsAsync(Arg.Is<EraEquipmentReferenceChanges>(changes =>
            changes.ModificationIds.SetEquals(new[] { referencedId }) && changes.TemplateIds.Count == 0), Arg.Any<CancellationToken>());
        await data.DidNotReceive().SaveCatalogAsync(Arg.Any<EraArmorCatalogDto>(), Arg.Any<EraArmorCatalogDto>(), Arg.Any<CancellationToken>());
        publication.Version.ShouldBe(7);
    }

}
