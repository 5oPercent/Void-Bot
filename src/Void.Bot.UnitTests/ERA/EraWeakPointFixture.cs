using System.Text.Json;
using AutoMapper;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Mappings;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWeakPointFixture
{
    [Fact]
    public void Should_default_legacy_weak_point_values_and_keep_templates_independent()
    {
        var framework = JsonSerializer.Deserialize<EraWeaponFrameworkDto>("{}")!;
        var template = JsonSerializer.Deserialize<EraWeaponTemplateDto>("{}")!;
        var enemy = JsonSerializer.Deserialize<EnemyDefinition>("{}")!;
        framework.WeakPointHealthDamageMultiplier.ShouldBe(1);
        framework.WeakPointProtectionDamageMultiplier.ShouldBe(1);
        template.WeakPointHealthDamageMultiplier.ShouldBe(1);
        template.WeakPointProtectionDamageMultiplier.ShouldBe(1);
        enemy.WeakPointHealthMultiplier.ShouldBe(1);
        enemy.WeakPointProtectionMultiplier.ShouldBe(1);

        var catalog = EraWeaponFixture.Catalog();
        catalog.Frameworks[0].WeakPointHealthDamageMultiplier = 5;
        catalog.Frameworks[0].WeakPointProtectionDamageMultiplier = .5;
        catalog.Templates[0].WeakPointHealthDamageMultiplier = 2;
        catalog.Templates[0].WeakPointProtectionDamageMultiplier = 0;
        EraWeaponConfigurationValidator.Validate(catalog);
        var copy = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(catalog))!;
        copy.Templates[0].WeakPointHealthDamageMultiplier.ShouldBe(2);
        copy.Templates[0].WeakPointProtectionDamageMultiplier.ShouldBe(0);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(10001)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Should_reject_invalid_coefficients_in_both_weapon_layers_and_enemy_pools(double value)
    {
        foreach (var layer in new[] { "Framework", "Template", "Enemy" })
        foreach (var pool in new[] { "Health", "Protection" })
        {
            var catalog = EraWeaponFixture.Catalog();
            var enemy = new EnemyDefinition { Id = "target", Name = "Target", SpeciesId = "construct", FactionId = "hostile" };
            object definition = layer == "Framework" ? catalog.Frameworks[0] : layer == "Template" ? catalog.Templates[0] : enemy;
            definition.GetType().GetProperty($"WeakPoint{pool}{(layer == "Enemy" ? "" : "Damage")}Multiplier")!.SetValue(definition, value);
            if (layer == "Enemy") Should.Throw<ArgumentException>(() => EnemyDefinitionValidator.Validate(enemy));
            else Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        }
    }

    [Fact]
    public void Should_map_both_coefficients_without_using_framework_values_for_template_reads()
    {
        var configuration = new MapperConfiguration(mapping => mapping.AddProfile<EraWeaponMapping>());
        configuration.AssertConfigurationIsValid();
        var mapper = configuration.CreateMapper();
        var framework = mapper.Map<EraWeaponFramework>(new EraWeaponFrameworkDto
            { WeakPointHealthDamageMultiplier = 2, WeakPointProtectionDamageMultiplier = .5 });
        mapper.Map<EraWeaponFrameworkDto>(framework).WeakPointHealthDamageMultiplier.ShouldBe(2);
        mapper.Map<EraWeaponFrameworkDto>(framework).WeakPointProtectionDamageMultiplier.ShouldBe(.5);
        var template = mapper.Map<EraWeaponTemplateDto>(new EraWeaponTemplate
            { WeakPointHealthDamageMultiplier = 0, WeakPointProtectionDamageMultiplier = 3 });
        template.WeakPointHealthDamageMultiplier.ShouldBe(0);
        template.WeakPointProtectionDamageMultiplier.ShouldBe(3);
    }
}
