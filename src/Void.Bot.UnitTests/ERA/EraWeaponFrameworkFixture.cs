using System.Net;
using System.Text.Json;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWeaponFrameworkFixture
{
    [Fact]
    public void Should_allow_melee_ammunition_and_infinite_reserve_and_penetration_without_gun_attributes()
    {
        var catalog = EraWeaponFixture.Catalog();
        var framework = catalog.Frameworks[0];
        framework.PresentationDriverKey = "Melee";
        framework.SupplementaryStats.Clear();
        framework.Capabilities = new() { new() { Kind = EraWeaponCapabilityKind.MeleeHit, MeleeHit = new() } };
        var stats = catalog.Templates[0].BaseStats;
        stats.Single(value => value.Stat == EraWeaponStat.Damage).Value = 0;
        stats.Single(value => value.Stat == EraWeaponStat.LoadedCapacity).Value = 1;
        stats.Single(value => value.Stat == EraWeaponStat.ReserveCapacity).Value = -1;
        stats.Single(value => value.Stat == EraWeaponStat.Penetration).Value = -1;
        stats.RemoveAll(value => value.Stat is EraWeaponStat.Accuracy or EraWeaponStat.Stability or EraWeaponStat.ReloadSpeed);
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        stats.Single(value => value.Stat == EraWeaponStat.LoadedCapacity).Value = -1;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        stats.Single(value => value.Stat == EraWeaponStat.LoadedCapacity).Value = 1; stats.Single(value => value.Stat == EraWeaponStat.ReserveCapacity).Value = -2;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        stats.Single(value => value.Stat == EraWeaponStat.ReserveCapacity).Value = -1; stats.Single(value => value.Stat == EraWeaponStat.Penetration).Value = -2;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Theory]
    [InlineData(EraWeaponStat.Accuracy)]
    [InlineData(EraWeaponStat.Stability)]
    [InlineData(EraWeaponStat.ReloadSpeed)]
    [InlineData(EraWeaponStat.ChargeTime)]
    [InlineData(EraWeaponStat.ProjectileSpeed)]
    public void Should_match_optional_values_to_framework_capabilities(EraWeaponStat stat)
    {
        var catalog = EraWeaponFixture.Catalog();
        var stats = catalog.Templates[0].BaseStats;
        if (stats.Any(value => value.Stat == stat)) stats.RemoveAll(value => value.Stat == stat);
        else stats.Add(new() { Stat = stat, Value = 1 });
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_reject_wrong_typed_capabilities_invalid_driver_keys_missing_effects_and_unknown_stat_id()
    {
        var catalog = EraWeaponFixture.Catalog();
        catalog.Frameworks[0].Capabilities[0].Recoil = new();
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog = EraWeaponFixture.Catalog();
        catalog.Frameworks[0].PresentationDriverKey = "invalid/key";
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog = EraWeaponFixture.Catalog();
        catalog.Templates[0].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Accuracy).EffectAtZero = null;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog = EraWeaponFixture.Catalog();
        catalog.Modifications[0].Modifiers[0].Stat = EraWeaponStat.Penetration;
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog.Modifications[0].Modifiers[0].Stat = (EraWeaponStat)12;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        ((int)EraWeaponStat.Penetration).ShouldBe(6);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponHitscanCapabilityDto>("{\"Script\":\"arbitrary\"}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponTemplateDto>("{\"Behavior\":{}}"));
    }

    [Theory]
    [InlineData("Firearm")]
    [InlineData("Bow")]
    [InlineData("Crossbow")]
    [InlineData("Melee")]
    [InlineData("custom.rifle-v2")]
    [InlineData("12345678901234567890123456789012")]
    public void Should_allow_custom_driver_keys_without_inferring_capabilities(string driver)
    {
        var framework = EraWeaponFixture.Catalog().Frameworks[0];
        framework.PresentationDriverKey = driver;
        Should.NotThrow(() => EraWeaponConfigurationValidator.ValidateFramework(framework));
        new GetEraWeaponFrameworksRequestValidator().Validate(new GetEraWeaponFrameworksRequest { PresentationDriverKey = driver, PageIndex = 1, PageSize = 10 }).IsValid.ShouldBeTrue();
        framework.Capabilities.RemoveAll(capability => capability.Kind == EraWeaponCapabilityKind.Hitscan);
        framework.Capabilities.Add(new() { Kind = EraWeaponCapabilityKind.Projectile, Projectile = new() { MaximumDistance = 30 } });
        framework.SupplementaryStats.Add(EraWeaponStat.ProjectileSpeed);
        Should.NotThrow(() => EraWeaponConfigurationValidator.ValidateFramework(framework));
        framework.Capabilities.Add(new() { Kind = EraWeaponCapabilityKind.MeleeHit, MeleeHit = new() });
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.ValidateFramework(framework));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(" rifle")]
    [InlineData("rifle ")]
    [InlineData("rifle\n")]
    [InlineData("rifle/key")]
    [InlineData("枪械")]
    [InlineData("123456789012345678901234567890123")]
    public void Should_validate_driver_key_format_in_configuration_and_filters(string key)
    {
        var framework = EraWeaponFixture.Catalog().Frameworks[0];
        framework.PresentationDriverKey = key;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.ValidateFramework(framework));
        new GetEraWeaponFrameworksRequestValidator().Validate(new GetEraWeaponFrameworksRequest { PresentationDriverKey = key, PageIndex = 1, PageSize = 10 }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Should_allow_same_named_frameworks_in_different_weapon_categories()
    {
        var catalog = EraWeaponFixture.Catalog();
        var framework = catalog.Frameworks[0];
        framework.Category.ShouldBe("未分类");
        framework.Name = "适配框架";
        framework.Category = "自动步枪";
        var other = JsonSerializer.Deserialize<EraWeaponFrameworkDto>(JsonSerializer.Serialize(framework))!;
        other.Id = Guid.NewGuid();
        other.Category = "脉冲步枪";
        catalog.Frameworks.Add(other);
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        foreach (var category in new[] { "自动步枪", "脉冲步枪", new string('字', 64) })
            new GetEraWeaponFrameworksRequestValidator().Validate(new GetEraWeaponFrameworksRequest { Category = category, PageIndex = 1, PageSize = 10 }).IsValid.ShouldBeTrue();
        foreach (var category in new[] { "", " ", " 自动步枪", "自动步枪 ", new string('字', 65) })
        {
            framework.Category = category;
            Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.ValidateFramework(framework));
            new GetEraWeaponFrameworksRequestValidator().Validate(new GetEraWeaponFrameworksRequest { Category = category, PageIndex = 1, PageSize = 10 }).IsValid.ShouldBeFalse();
        }
    }

    [Fact]
    public async Task Should_disable_new_references_without_stopping_existing_templates_or_mutating_the_previous_catalog()
    {
        var catalog = EraWeaponFixture.Catalog();
        var (service, data, publication) = Service(catalog);
        var result = await service.DisableFrameworkAsync(new() { Id = catalog.Frameworks[0].Id, ExpectedManifestVersion = 7 }, CancellationToken.None);
        result.Data!.Configuration.Frameworks[0].IsEnabled.ShouldBeFalse();
        result.Data.Configuration.Templates[0].IsGenerationEnabled.ShouldBeTrue();
        catalog.Frameworks[0].IsEnabled.ShouldBeTrue();
        publication.Version.ShouldBe(8);
        await data.DidNotReceiveWithAnyArgs().GetReferencedWeaponsAsync(default!, default);
        await data.DidNotReceiveWithAnyArgs().GetPendingRewardPlansAsync(default);

        var disabled = result.Data.Configuration;
        var (next, _, _) = Service(disabled);
        var copy = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(disabled.Templates[0]))!;
        copy.Id = Guid.NewGuid();
        var error = await Should.ThrowAsync<EraWeaponException>(() => next.SaveTemplateAsync(new() { Template = copy, ExpectedManifestVersion = 7 }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Should_protect_referenced_frameworks_and_publish_an_unreferenced_deletion_once()
    {
        var catalog = EraWeaponFixture.Catalog();
        var (service, data, publication) = Service(catalog);
        var error = await Should.ThrowAsync<EraWeaponException>(() => service.DeleteFrameworkAsync(new()
            { Id = catalog.Frameworks[0].Id, ExpectedManifestVersion = 7 }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        publication.Version.ShouldBe(7);
        await data.DidNotReceiveWithAnyArgs().SaveCatalogAsync(default!, default!, default);
        var unused = JsonSerializer.Deserialize<EraWeaponFrameworkDto>(JsonSerializer.Serialize(catalog.Frameworks[0]))!;
        unused.Id = Guid.NewGuid(); catalog.Frameworks.Add(unused);
        var result = await service.DeleteFrameworkAsync(new() { Id = unused.Id, ExpectedManifestVersion = 7 }, CancellationToken.None);
        result.Data!.Version.ShouldBe(8);
        result.Data.Configuration.Frameworks.Count.ShouldBe(1);
        await data.Received(1).SaveCatalogAsync(catalog, Arg.Any<EraWeaponCatalogDto>(), CancellationToken.None);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Should_protect_unselected_old_candidates_when_removing_a_supplementary_attribute(bool pendingPlan)
    {
        var catalog = EraWeaponFixture.Catalog();
        var proposed = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(catalog))!;
        // The obsolete modifier is no longer in a pool, but remains an unselected candidate on an old asset.
        var definition = proposed.Modifications[1];
        definition.Modifiers[0].Stat = EraWeaponStat.Accuracy;
        catalog.Modifications[1].Modifiers[0].Stat = EraWeaponStat.Accuracy;
        foreach (var slot in proposed.SlotConfigurations[0].Slots)
            slot.Pool.RemoveAll(entry => entry.ModDefinitionId == definition.Id);
        proposed.Frameworks[0].Capabilities.RemoveAll(capability => capability.Kind == EraWeaponCapabilityKind.Aim);
        proposed.Frameworks[0].SupplementaryStats.Remove(EraWeaponStat.Accuracy);
        proposed.Templates[0].BaseStats.RemoveAll(value => value.Stat == EraWeaponStat.Accuracy);
        var instance = EraWeaponGenerator.Generate(catalog.SlotConfigurations[0], _ => 0);
        instance.Slots[0].SelectedCandidateId = instance.Slots[0].Candidates[0].CandidateId;
        var asset = new EraEquipmentInstance { Id = Guid.NewGuid(), TemplateId = catalog.Templates[0].Id };
        var (service, data, publication) = Service(catalog);
        if (pendingPlan)
        {
            var plan = new Void.Bot.Core.Services.ERA.Reward.Models.EraInventoryRewardPlan
            {
                Grants = new() { new() { Equipment = new() { new() { EquipmentType = EraEquipmentType.Weapon, TemplateId = asset.TemplateId, Weapon = instance } } } }
            };
            data.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<string> { JsonSerializer.Serialize(plan) }));
        }
        else
        {
            data.GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<EraEquipmentInstance> { asset }));
            data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new Dictionary<Guid, EraWeaponInstanceDto> { [asset.Id] = instance }));
        }
        var error = await Should.ThrowAsync<EraWeaponException>(() => service.PublishConfigurationAsync(new()
            { ExpectedManifestVersion = 7, Configuration = proposed }, CancellationToken.None));
        error.Code.ShouldBe(HttpStatusCode.Conflict);
        publication.Version.ShouldBe(7);
        await data.DidNotReceiveWithAnyArgs().SaveCatalogAsync(default!, default!, default);
    }

    [Fact]
    public void Should_reject_obsolete_mapping_contracts_and_allow_frameworks_without_templates()
    {
        var catalog = EraWeaponFixture.Catalog();
        JsonSerializer.SerializeToElement(catalog.Frameworks[0]).TryGetProperty("StatMappingId", out _).ShouldBeFalse();
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponFrameworkDto>("{\"StatMappingId\":\"40000000-0000-0000-0000-000000000001\"}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponCatalogDto>("{\"StatMappings\":[]}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponTemplateDto>("{\"StatMappingId\":\"40000000-0000-0000-0000-000000000001\"}"));
        catalog.Templates.Clear();
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    private static (EraWeaponService Service, IEraWeaponDataProvider Data, EraEquipmentConfiguration Publication) Service(EraWeaponCatalogDto catalog)
    {
        var data = Substitute.For<IEraWeaponDataProvider>();
        var equipment = Substitute.For<IEraEquipmentDataProvider>();
        var publication = new EraEquipmentConfiguration { Version = 7, Payload = "{\"SchemaVersion\":3}" };
        data.LockPublicationAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult<EraEquipmentConfiguration?>(publication));
        data.GetCatalogAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(catalog));
        equipment.GetArmorCatalogAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new EraArmorCatalogDto()));
        data.GetReferencedWeaponsAsync(Arg.Any<EraEquipmentReferenceChanges>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<EraEquipmentInstance>()));
        data.GetInstancesAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(new Dictionary<Guid, EraWeaponInstanceDto>()));
        data.GetPendingRewardPlansAsync(Arg.Any<CancellationToken>()).Returns(Task.FromResult(new List<string>()));
        var user = Substitute.For<ICurrentUser>(); user.Id.Returns((Guid?)Guid.NewGuid());
        return (new EraWeaponService(data, equipment, user, Substitute.For<ILocalizationService>(), TimeProvider.System), data, publication);
    }
}
