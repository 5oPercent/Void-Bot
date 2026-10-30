using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    private static EraWeaponCatalogDto WeaponCatalog()
    {
        var catalog = new EraWeaponCatalogDto();
        catalog.Frameworks.Add(new EraWeaponFrameworkDto
        {
            Name = "Rifle framework", PresentationDriverKey = "Firearm",
            SupplementaryStats = new() { EraWeaponStat.Accuracy, EraWeaponStat.Stability, EraWeaponStat.ReloadSpeed },
            Capabilities = new()
            {
                new() { Kind = EraWeaponCapabilityKind.Aim, Aim = new() },
                new() { Kind = EraWeaponCapabilityKind.Recoil, Recoil = new() },
                new() { Kind = EraWeaponCapabilityKind.Reload, Reload = new() { Mode = EraWeaponReloadMode.Magazine, BaseSeconds = 2.4 } },
                new() { Kind = EraWeaponCapabilityKind.Firing, Firing = new() { Mode = EraWeaponFireMode.Automatic } },
                new() { Kind = EraWeaponCapabilityKind.Hitscan, Hitscan = new() { MaximumDistance = 30 } }
            }
        });
        var configuration = new EraWeaponSlotConfigurationDto { Id = Guid.NewGuid(), Name = "Rifle slots" };
        catalog.SlotConfigurations.Add(configuration);
        var template = new EraWeaponTemplateDto
        {
            Id = Guid.Parse("20000000-0000-0000-0000-000000000001"), Name = "Integration rifle", PresentationKey = "scifi_rifle",
            SlotConfigurationId = configuration.Id, FrameworkId = catalog.Frameworks[0].Id,
            CompatibleSlots = new List<EraEquipmentSlot> { EraEquipmentSlot.Weapon1, EraEquipmentSlot.Weapon2, EraEquipmentSlot.Weapon3 },
            BaseStats = new()
            {
                new() { Stat = EraWeaponStat.Damage, Value = 50 },
                new() { Stat = EraWeaponStat.Frequency, Value = 5 },
                new() { Stat = EraWeaponStat.LoadedCapacity, Value = 30 },
                new() { Stat = EraWeaponStat.ReserveCapacity, Value = 120 },
                new() { Stat = EraWeaponStat.ReloadSpeed, Value = 1 },
                new() { Stat = EraWeaponStat.Penetration, Value = 0 },
                new() { Stat = EraWeaponStat.Accuracy, Value = 50, EffectAtZero = 3, EffectAtHundred = 1 },
                new() { Stat = EraWeaponStat.Stability, Value = 50, EffectAtZero = 2, EffectAtHundred = .5 },
                new() { Stat = EraWeaponStat.Handling, Value = 50, EffectAtZero = 1, EffectAtHundred = .35 }
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
            for (var index = 0; index < 2; index++)
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

    private Task<PublishEraWeaponConfigurationResponse> PublishWeaponsAsync(EraWeaponCatalogDto catalog, long version = 7,
        EraIntegrationSqlInterceptor? interceptor = null) => RequestAsync((mediator, token) =>
        mediator.SendAsync<PublishEraWeaponConfigurationCommand, PublishEraWeaponConfigurationResponse>(
            new PublishEraWeaponConfigurationCommand { ExpectedManifestVersion = version, Configuration = catalog }, token), interceptor);

    private Task<GetEraCharacterEquipmentResponse> ReadWeaponEquipmentAsync() => RequestAsync((mediator, token) =>
        mediator.RequestAsync<GetEraCharacterEquipmentRequest, GetEraCharacterEquipmentResponse>(
            new GetEraCharacterEquipmentRequest { CharacterId = _characterId }, token));

    private Task<SelectEraWeaponModificationResponse> SelectWeaponAsync(SelectEraWeaponModificationCommand command,
        EraIntegrationSqlInterceptor? interceptor = null) => RequestAsync((mediator, token) =>
        mediator.SendAsync<SelectEraWeaponModificationCommand, SelectEraWeaponModificationResponse>(command, token), interceptor);

    private async Task<Guid> PrepareWeaponRewardAsync()
    {
        await ConfigureRewardsAsync();
        await ReadDatabaseAsync(async context =>
        {
            var configuration = await context.Set<EraRewardConfiguration>().SingleAsync(row => row.Id == RewardConfigurationId);
            var rules = JsonSerializer.Deserialize<EraRewardRules>(configuration.Payload)!;
            rules.Pools[0].Entries[0].TemplateId = (await context.Set<EraWeaponTemplate>().SingleAsync()).Id;
            configuration.Payload = JsonSerializer.Serialize(rules);
            await context.SaveChangesAsync();
        });
        return (await PrepareRewardAsync(RewardCommand())).Data;
    }

    [Fact]
    public async Task Template_effect_endpoints_should_publish_and_reject_stale_writes()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        await PublishWeaponsAsync(catalog);
        var template = catalog.Templates[0];
        template.BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtZero = .8;
        var saved = await RequestAsync((mediator, token) => mediator.SendAsync<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>(
            new SaveEraWeaponTemplateCommand { ExpectedManifestVersion = 8, Template = template }, token));
        saved.Data!.Version.ShouldBe(9);
        saved.Data.Configuration.Templates.Single().BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtZero.ShouldBe(.8);
        JsonSerializer.Serialize(saved.Data.Configuration.Modifications.OrderBy(row => row.Id)).ShouldBe(JsonSerializer.Serialize(catalog.Modifications.OrderBy(row => row.Id)));
        var stale = await Should.ThrowAsync<EraWeaponException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>(
                new SaveEraWeaponTemplateCommand { ExpectedManifestVersion = 8, Template = template }, token)));
        stale.Code.ShouldBe(HttpStatusCode.Conflict);
        stale.ErrorCode.ShouldBe("era.weapon_config_version_conflict");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentConfiguration>().SingleAsync(row => row.Id == ConfigurationId)).Version.ShouldBe(9);
            var handling = await context.Set<EraWeaponTemplateStat>().SingleAsync(entry => entry.Stat == EraWeaponStat.Handling);
            handling.EffectAtZero.ShouldBe(.8);
            handling.EffectAtHundred.ShouldBe(.35);
        });
    }

    [Fact]
    public async Task Invalid_template_effect_endpoints_should_leave_the_publication_unchanged()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        await PublishWeaponsAsync(catalog);
        catalog.Templates[0].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtHundred = 0;
        var invalid = await Should.ThrowAsync<EraWeaponException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<SaveEraWeaponTemplateCommand, SaveEraWeaponTemplateResponse>(
                new SaveEraWeaponTemplateCommand { ExpectedManifestVersion = 8, Template = catalog.Templates[0] }, token)));
        invalid.Code.ShouldBe(HttpStatusCode.BadRequest);
        invalid.ErrorCode.ShouldBe("era.weapon_invalid_configuration");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentConfiguration>().SingleAsync(row => row.Id == ConfigurationId)).Version.ShouldBe(8);
            (await context.Set<EraWeaponTemplateStat>().SingleAsync(entry => entry.Stat == EraWeaponStat.Handling)).EffectAtHundred.ShouldBe(.35);
        });
    }

    [Fact]
    public async Task Weapons_and_armor_should_compete_for_the_same_120_inventory_capacity()
    {
        await SeedAsync(119);
        await PublishWeaponsAsync(WeaponCatalog());
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var state = (await ReadWeaponEquipmentAsync()).Data!;
        state.TotalInventoryCount.ShouldBe(120);
        state.InventoryItems.Count(value => value.EquipmentType == EraEquipmentType.Armor).ShouldBe(119);
        var filtered = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraCharacterInventoryRequest, GetEraCharacterInventoryResponse>(
            new GetEraCharacterInventoryRequest { CharacterId = _characterId, EquipmentType = EraEquipmentType.Weapon }, token));
        filtered.Data!.InventoryItems.Count.ShouldBe(1);
        filtered.Data.TotalInventoryCount.ShouldBe(120);
        var pending = await PrepareWeaponRewardAsync();
        (await Should.ThrowAsync<EraRewardException>(() => GrantRewardAsync(pending))).ErrorCode.ShouldBe("era.reward_inventory_full");
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentInstance>().CountAsync(value => value.CharacterId == _characterId && value.Location == EraEquipmentLocation.Inventory)).ShouldBe(120);
            (await context.Set<EraRewardEvent>().SingleAsync(value => value.Id == pending)).ResultPayload.ShouldBeNull();
        });
    }

    [Fact]
    public async Task Weapon_public_messages_should_generate_fixed_candidates_follow_new_stats_and_replay_selection()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        (await PublishWeaponsAsync(catalog)).Data!.Version.ShouldBe(8);
        var eventId = await PrepareWeaponRewardAsync();
        var grant = await GrantRewardAsync(eventId);
        var itemId = grant.Data!.Grants.Single().EquipmentInstanceIds.Single();
        var first = (await ReadWeaponEquipmentAsync()).Data!;
        var item = first.InventoryItems.Single();
        item.Id.ShouldBe(itemId);
        item.Weapon!.Slots.ShouldAllBe(slot => slot.Candidates.Count == 2);
        var before = JsonSerializer.Serialize(item.Weapon.Slots.Select(slot => slot.Candidates));
        var slot = item.Weapon.Slots[0];
        var command = new SelectEraWeaponModificationCommand
        {
            CharacterId = _characterId, EquipmentInstanceId = itemId, RequestId = Guid.NewGuid(), SlotId = slot.SlotId,
            CandidateId = slot.Candidates.Single(candidate => candidate.CandidateId != slot.SelectedCandidateId).CandidateId,
            ExpectedManifestVersion = 8, ExpectedEquipmentVersion = 4, ExpectedItemVersion = 1
        };
        (await SelectWeaponAsync(command)).Data!.State.EquipmentVersion.ShouldBe(5);
        catalog.Templates[0].BaseStats.Single(value => value.Stat == EraWeaponStat.Damage).Value = 55;
        catalog.Templates[0].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Accuracy).EffectAtHundred = .5;
        foreach (var definition in catalog.SlotConfigurations[0].Slots) { definition.CandidateCount = 1; definition.Pool.RemoveAt(0); }
        (await PublishWeaponsAsync(catalog, 8)).Data!.Version.ShouldBe(9);
        var current = (await ReadWeaponEquipmentAsync()).Data!;
        current.ManifestVersion.ShouldBe(9);
        current.EquipmentVersion.ShouldBe(5);
        var updatedManifest = await RequestAsync((mediator, token) => mediator.RequestAsync<GetEraEquipmentManifestRequest, GetEraEquipmentManifestResponse>(
            new GetEraEquipmentManifestRequest(), token));
        updatedManifest.Data!.Version.ShouldBe(9);
        updatedManifest.Data.Weapons.Templates.Single().BaseStats.Single(entry => entry.Stat == EraWeaponStat.Accuracy).EffectAtHundred.ShouldBe(.5);
        item = current.InventoryItems.Single();
        item.Version.ShouldBe(2);
        item.GenerationManifestVersion.ShouldBe(8);
        item.WeaponTemplate!.BaseStats.Single(value => value.Stat == EraWeaponStat.Damage).Value.ShouldBe(55);
        JsonSerializer.Serialize(item.Weapon!.Slots.Select(value => value.Candidates)).ShouldBe(before);
        item.Weapon.Slots[0].SelectedCandidateId.ShouldBe(command.CandidateId);
        var replay = await SelectWeaponAsync(command);
        replay.IsReplay.ShouldBeTrue();
        replay.Data!.State.ManifestVersion.ShouldBe(8); // 原回执保留其目录，客户端不能用它覆盖较新目录。
        var disabled = await RequestAsync((mediator, token) => mediator.SendAsync<DisableEraWeaponTemplateCommand, DisableEraWeaponTemplateResponse>(
            new DisableEraWeaponTemplateCommand { Id = catalog.Templates[0].Id, ExpectedManifestVersion = 9 }, token));
        disabled.Data!.Version.ShouldBe(10);
        disabled.Data.Configuration.Templates.Single().IsGenerationEnabled.ShouldBeFalse();
        disabled.Data.Configuration.Templates.Single().BaseStats.Single(entry => entry.Stat == EraWeaponStat.Accuracy).EffectAtHundred.ShouldBe(.5);
        (await ReadWeaponEquipmentAsync()).Data!.InventoryItems.Single().Id.ShouldBe(itemId);
        await Should.ThrowAsync<EraWeaponException>(() => RequestAsync((mediator, token) =>
            mediator.SendAsync<DeleteEraWeaponTemplateCommand, DeleteEraWeaponTemplateResponse>(
                new DeleteEraWeaponTemplateCommand { Id = catalog.Templates[0].Id, ExpectedManifestVersion = 10 }, token)));
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraWeaponInstanceModCandidate>().CountAsync(row => row.EquipmentInstanceId == itemId)).ShouldBe(10);
            (await context.Set<EraArmorInstance>().AnyAsync(row => row.EquipmentInstanceId == itemId)).ShouldBeFalse();
            (await context.Set<EraEquipmentOperationReceipt>().CountAsync(row => row.RequestId == command.RequestId)).ShouldBe(1);
        });
    }

    [Fact]
    public async Task Weapon_selection_should_roll_back_selection_item_and_character_when_receipt_write_fails()
    {
        await SeedAsync(0);
        await PublishWeaponsAsync(WeaponCatalog());
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var item = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems.Single();
        var slot = item.Weapon!.Slots[0];
        var command = new SelectEraWeaponModificationCommand
        {
            CharacterId = _characterId, EquipmentInstanceId = item.Id, RequestId = Guid.NewGuid(), SlotId = slot.SlotId,
            CandidateId = slot.Candidates.Single(candidate => candidate.CandidateId != slot.SelectedCandidateId).CandidateId,
            ExpectedManifestVersion = 8, ExpectedEquipmentVersion = 4, ExpectedItemVersion = 1
        };
        var interceptor = new EraIntegrationSqlInterceptor { FailReceiptInsert = true };
        await Should.ThrowAsync<Exception>(() => SelectWeaponAsync(command, interceptor));
        interceptor.ReceiptFailureInjected.ShouldBeTrue();
        var after = (await ReadWeaponEquipmentAsync()).Data!;
        after.EquipmentVersion.ShouldBe(4);
        after.InventoryItems.Single().Version.ShouldBe(1);
        after.InventoryItems.Single().Weapon!.Slots[0].SelectedCandidateId.ShouldBe(slot.SelectedCandidateId);
        await ReadDatabaseAsync(async context =>
            (await context.Set<EraEquipmentOperationReceipt>().AnyAsync(row => row.RequestId == command.RequestId)).ShouldBeFalse());
    }

    [Fact]
    public async Task Weapon_publication_should_protect_pending_reward_references_and_roll_back_partial_config_writes()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        await PublishWeaponsAsync(catalog);
        var eventId = await PrepareWeaponRewardAsync();
        var originalCatalog = JsonSerializer.Serialize(catalog);
        var removed = catalog.SlotConfigurations[0].Slots[0].Pool[0].ModDefinitionId;
        catalog.SlotConfigurations[0].Slots[0].Pool.RemoveAt(0);
        catalog.SlotConfigurations[0].Slots[0].CandidateCount = 1;
        catalog.Modifications.RemoveAll(value => value.Id == removed);
        await Should.ThrowAsync<EraWeaponException>(() => PublishWeaponsAsync(catalog, 8));
        catalog = JsonSerializer.Deserialize<EraWeaponCatalogDto>(originalCatalog)!;
        catalog.Templates[0].BaseStats.Single(value => value.Stat == EraWeaponStat.Damage).Value = 99;
        catalog.SlotConfigurations[0].Slots[0].CandidateCount = 1;
        catalog.SlotConfigurations[0].Slots[0].Pool[0].Weight = 99;
        var interceptor = new EraIntegrationSqlInterceptor { FailWeaponConfigurationInsert = true };
        await Should.ThrowAsync<Exception>(() => PublishWeaponsAsync(catalog, 8, interceptor));
        interceptor.WeaponConfigurationFailureInjected.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
        {
            (await context.Set<EraEquipmentConfiguration>().SingleAsync(row => row.Id == ConfigurationId)).Version.ShouldBe(8);
            (await context.Set<EraWeaponTemplateStat>().SingleAsync(row => row.Stat == EraWeaponStat.Damage)).Value.ShouldBe(50);
            (await context.Set<EraWeaponConfigurationModPool>().CountAsync()).ShouldBe(10);
            (await context.Set<EraWeaponConfigurationModPool>().SumAsync(row => row.Weight)).ShouldBe(15);
            (await context.Set<EraWeaponConfigurationSlot>().ToListAsync()).ShouldAllBe(value => value.CandidateCount == 2);
        });
        (await GrantRewardAsync(eventId)).Data!.Grants.Single().EquipmentInstanceIds.Count.ShouldBe(1);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Weapon_template_edit_should_not_reinsert_unchanged_configuration_slots(bool rename)
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        await PublishWeaponsAsync(catalog);
        string originalSlots = string.Empty;
        string originalPools = string.Empty;
        await ReadDatabaseAsync(async context =>
        {
            originalSlots = JsonSerializer.Serialize(await context.Set<EraWeaponConfigurationSlot>().OrderBy(row => row.Id).ToListAsync());
            originalPools = JsonSerializer.Serialize(await context.Set<EraWeaponConfigurationModPool>()
                .OrderBy(row => row.SlotId).ThenBy(row => row.ModDefinitionId).ToListAsync());
        });
        if (rename) catalog.Templates[0].Name = "Renamed";
        else catalog.Templates[0].BaseStats.Single(value => value.Stat == EraWeaponStat.Damage).Value = 99;
        var interceptor = new EraIntegrationSqlInterceptor { FailWeaponConfigurationInsert = true, FailWeaponTemplateStatInsert = rename };
        var response = await PublishWeaponsAsync(catalog, 8, interceptor);
        response.Data!.Version.ShouldBe(9);
        interceptor.WeaponConfigurationFailureInjected.ShouldBeFalse();
        await ReadDatabaseAsync(async context =>
        {
            var template = await context.Set<EraWeaponTemplate>().SingleAsync();
            (await context.Set<EraWeaponTemplateStat>().SingleAsync(row => row.TemplateId == template.Id && row.Stat == EraWeaponStat.Damage)).Value
                .ShouldBe(catalog.Templates[0].BaseStats.Single(value => value.Stat == EraWeaponStat.Damage).Value);
            template.Name.ShouldBe(catalog.Templates[0].Name);
            JsonSerializer.Serialize(await context.Set<EraWeaponConfigurationSlot>().OrderBy(row => row.Id).ToListAsync()).ShouldBe(originalSlots);
            JsonSerializer.Serialize(await context.Set<EraWeaponConfigurationModPool>()
                .OrderBy(row => row.SlotId).ThenBy(row => row.ModDefinitionId).ToListAsync()).ShouldBe(originalPools);
        });
    }

    [Fact]
    public async Task Weapon_selection_should_reject_foreign_slot_candidates_and_keep_repeated_current_selection_a_no_op()
    {
        await SeedAsync(0);
        await PublishWeaponsAsync(WeaponCatalog());
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var item = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems.Single();
        var slot = item.Weapon!.Slots[0];
        var command = new SelectEraWeaponModificationCommand
        {
            CharacterId = _characterId, EquipmentInstanceId = item.Id, RequestId = Guid.NewGuid(), SlotId = slot.SlotId,
            CandidateId = item.Weapon.Slots[1].SelectedCandidateId,
            ExpectedManifestVersion = 8, ExpectedEquipmentVersion = 4, ExpectedItemVersion = 1
        };
        (await SelectWeaponAsync(command)).Code.ShouldBe(HttpStatusCode.BadRequest);
        command.CandidateId = slot.SelectedCandidateId;
        var noOp = await SelectWeaponAsync(command);
        noOp.Code.ShouldBe(HttpStatusCode.OK);
        noOp.Data!.State.EquipmentVersion.ShouldBe(4);
        noOp.Data.State.InventoryItems.Single().Version.ShouldBe(1);
        (await SelectWeaponAsync(command)).IsReplay.ShouldBeTrue();
        command.CandidateId = slot.Candidates.Single(value => value.CandidateId != slot.SelectedCandidateId).CandidateId;
        (await SelectWeaponAsync(command)).Code.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Concurrent_weapon_selections_should_commit_only_one_asset_version_change()
    {
        await SeedAsync(0);
        await PublishWeaponsAsync(WeaponCatalog());
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var item = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems.Single();
        var slot = item.Weapon!.Slots[0];
        var candidate = slot.Candidates.Single(value => value.CandidateId != slot.SelectedCandidateId).CandidateId;
        var commands = Enumerable.Range(0, 2).Select(_ => new SelectEraWeaponModificationCommand
        {
            CharacterId = _characterId, EquipmentInstanceId = item.Id, RequestId = Guid.NewGuid(), SlotId = slot.SlotId, CandidateId = candidate,
            ExpectedManifestVersion = 8, ExpectedEquipmentVersion = 4, ExpectedItemVersion = 1
        }).ToList();
        var responses = await Task.WhenAll(commands.Select(command => SelectWeaponAsync(command)));
        responses.Count(response => response.Code == HttpStatusCode.OK).ShouldBe(1);
        responses.Count(response => response.Code == HttpStatusCode.Conflict).ShouldBe(1);
        var state = (await ReadWeaponEquipmentAsync()).Data!;
        state.EquipmentVersion.ShouldBe(5);
        state.InventoryItems.Single().Version.ShouldBe(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Weapon_selection_database_constraint_should_reject_a_candidate_from_another_slot_or_weapon(bool otherWeapon)
    {
        await SeedAsync(0);
        await PublishWeaponsAsync(WeaponCatalog());
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        if (otherWeapon) await GrantRewardAsync(await PrepareWeaponRewardAsync());
        await ReadDatabaseAsync(async context =>
        {
            var selections = await context.Set<EraWeaponInstanceModSelection>().OrderBy(value => value.SlotId).ToListAsync();
            var first = selections[0];
            first.SelectedCandidateId = otherWeapon
                ? selections.First(value => value.EquipmentInstanceId != first.EquipmentInstanceId && value.SlotId == first.SlotId).SelectedCandidateId
                : selections.First(value => value.EquipmentInstanceId == first.EquipmentInstanceId && value.SlotId != first.SlotId).SelectedCandidateId;
            await Should.ThrowAsync<DbUpdateException>(() => context.SaveChangesAsync());
        });
        (await ReadWeaponEquipmentAsync()).Code.ShouldBe(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Shared_scheme_changes_should_affect_new_weapons_and_preserve_removed_instance_slots()
    {
        await SeedAsync(0);
        var catalog = WeaponCatalog();
        var configuration = catalog.SlotConfigurations[0];
        var extra = JsonSerializer.Deserialize<EraWeaponConfigurationSlotDto>(JsonSerializer.Serialize(configuration.Slots[0]))!;
        extra.Id = Guid.NewGuid(); extra.Name = "Secondary optic"; extra.DisplayOrder = 5;
        configuration.Slots.Add(extra);
        await PublishWeaponsAsync(catalog);
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var original = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems.Single();
        original.Weapon!.Slots.Count.ShouldBe(6);
        var snapshot = JsonSerializer.Serialize(original.Weapon);
        configuration.Slots.Remove(extra);
        for (var index = 0; index < 2; index++)
        {
            var slot = new EraWeaponConfigurationSlotDto
            {
                Name = $"New optic {index}", SlotTypeId = extra.SlotTypeId, CandidateCount = extra.CandidateCount,
                DisplayOrder = index + 5, Pool = extra.Pool.ToList()
            };
            configuration.Slots.Add(slot);
        }
        var saved = await RequestAsync((mediator, token) =>
            mediator.SendAsync<SaveEraWeaponSlotConfigurationCommand, SaveEraWeaponSlotConfigurationResponse>(
                new SaveEraWeaponSlotConfigurationCommand
                {
                    ExpectedManifestVersion = 8, SlotConfiguration = configuration
                }, token));
        saved.Data!.Version.ShouldBe(9);
        configuration.Slots.ShouldAllBe(slot => slot.Id != Guid.Empty);
        var state = (await ReadWeaponEquipmentAsync()).Data!;
        var old = state.InventoryItems.Single();
        JsonSerializer.Serialize(old.Weapon).ShouldBe(snapshot);
        var removed = old.Weapon!.Slots.Single(slot => slot.SlotId == extra.Id);
        var candidateId = removed.Candidates.Single(candidate => candidate.CandidateId != removed.SelectedCandidateId).CandidateId;
        var selected = await SelectWeaponAsync(new SelectEraWeaponModificationCommand
        {
            CharacterId = _characterId, EquipmentInstanceId = old.Id, RequestId = Guid.NewGuid(), SlotId = removed.SlotId,
            CandidateId = candidateId, ExpectedManifestVersion = 9, ExpectedEquipmentVersion = state.EquipmentVersion,
            ExpectedItemVersion = old.Version
        });
        selected.Code.ShouldBe(HttpStatusCode.OK);
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var items = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems;
        items.Single(item => item.Id != old.Id).Weapon!.Slots.Count.ShouldBe(7);
        items.Single(item => item.Id == old.Id).Weapon!.Slots.Count.ShouldBe(6);
        catalog.Templates[0].SlotConfigurationId = null;
        (await PublishWeaponsAsync(catalog, 9)).Data!.Version.ShouldBe(10);
        await GrantRewardAsync(await PrepareWeaponRewardAsync());
        var current = (await ReadWeaponEquipmentAsync()).Data!.InventoryItems;
        current.Single(item => items.All(previous => previous.Id != item.Id)).Weapon!.Slots.ShouldBeEmpty();
        current.Single(item => item.Id == old.Id).Weapon!.Slots.Count.ShouldBe(6);
        await ReadDatabaseAsync(async context =>
            (await context.Set<EraWeaponTemplate>().SingleAsync()).SlotConfigurationId.ShouldBeNull());
    }
}
