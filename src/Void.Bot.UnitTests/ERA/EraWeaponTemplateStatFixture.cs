using System.Text.Json;
using Shouldly;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraWeaponTemplateStatFixture
{
    [Theory]
    [InlineData(EraWeaponStat.Damage)]
    [InlineData(EraWeaponStat.Frequency)]
    [InlineData(EraWeaponStat.Handling)]
    [InlineData(EraWeaponStat.LoadedCapacity)]
    [InlineData(EraWeaponStat.ReserveCapacity)]
    [InlineData(EraWeaponStat.Penetration)]
    public void Should_require_every_common_attribute_as_an_explicit_entry(EraWeaponStat stat)
    {
        var catalog = EraWeaponFixture.Catalog();
        catalog.Templates[0].BaseStats.RemoveAll(value => value.Stat == stat);
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Theory]
    [InlineData(EraWeaponStat.LoadedCapacity, 1.5)]
    [InlineData(EraWeaponStat.ReserveCapacity, 1.5)]
    [InlineData(EraWeaponStat.Penetration, 1.5)]
    [InlineData(EraWeaponStat.ReserveCapacity, -1.5)]
    [InlineData(EraWeaponStat.Penetration, -1.5)]
    [InlineData(EraWeaponStat.LoadedCapacity, 0)]
    [InlineData(EraWeaponStat.Damage, -1)]
    [InlineData(EraWeaponStat.Damage, double.NaN)]
    [InlineData(EraWeaponStat.Damage, double.PositiveInfinity)]
    [InlineData(EraWeaponStat.Frequency, 0)]
    [InlineData(EraWeaponStat.Handling, 101)]
    [InlineData(EraWeaponStat.Accuracy, -1)]
    [InlineData(EraWeaponStat.Stability, 101)]
    [InlineData(EraWeaponStat.ReloadSpeed, 0)]
    [InlineData(EraWeaponStat.Damage, 1000001)]
    public void Should_preserve_attribute_specific_bounds_with_a_shared_numeric_value(EraWeaponStat stat, double value)
    {
        var catalog = EraWeaponFixture.Catalog();
        catalog.Templates[0].BaseStats.Single(entry => entry.Stat == stat).Value = value;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_reject_duplicate_and_unknown_attribute_entries()
    {
        var catalog = EraWeaponFixture.Catalog();
        catalog.Templates[0].BaseStats.Add(new() { Stat = EraWeaponStat.Damage, Value = 10 });
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        catalog.Templates[0].BaseStats.RemoveAt(catalog.Templates[0].BaseStats.Count - 1);
        catalog.Templates[0].BaseStats[0].Stat = (EraWeaponStat)12;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_round_trip_entries_and_reject_legacy_objects_missing_values_and_unknown_fields()
    {
        var template = EraWeaponFixture.Catalog().Templates[0];
        var payload = JsonSerializer.SerializeToElement(template);
        payload.GetProperty("BaseStats").ValueKind.ShouldBe(JsonValueKind.Array);
        var restored = JsonSerializer.Deserialize<EraWeaponTemplateDto>(payload.GetRawText())!;
        restored.BaseStats.Single(entry => entry.Stat == EraWeaponStat.Damage).Value.ShouldBe(50);
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponTemplateDto>("{\"BaseStats\":{\"Damage\":50}}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponStatValueDto>("{\"Stat\":1}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponStatValueDto>("{\"Value\":1}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraWeaponStatValueDto>("{\"Stat\":1,\"Value\":50,\"Damage\":50}"));
    }

    [Theory]
    [InlineData(EraWeaponStat.Accuracy)]
    [InlineData(EraWeaponStat.Stability)]
    [InlineData(EraWeaponStat.Handling)]
    public void Should_require_both_score_endpoints(EraWeaponStat stat)
    {
        foreach (var missingZero in new[] { false, true })
        {
            var catalog = EraWeaponFixture.Catalog();
            var entry = catalog.Templates[0].BaseStats.Single(value => value.Stat == stat);
            if (missingZero) entry.EffectAtZero = null;
            else entry.EffectAtHundred = null;
            Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        }
    }

    [Theory]
    [InlineData(EraWeaponStat.Damage)]
    [InlineData(EraWeaponStat.Frequency)]
    [InlineData(EraWeaponStat.LoadedCapacity)]
    [InlineData(EraWeaponStat.ReserveCapacity)]
    [InlineData(EraWeaponStat.Penetration)]
    [InlineData(EraWeaponStat.ReloadSpeed)]
    [InlineData(EraWeaponStat.ProjectileSpeed)]
    [InlineData(EraWeaponStat.ChargeTime)]
    public void Should_reject_endpoints_for_direct_value_attributes(EraWeaponStat stat)
    {
        var catalog = EraWeaponFixture.Catalog();
        var framework = catalog.Frameworks[0];
        if (stat == EraWeaponStat.ProjectileSpeed)
        {
            framework.Capabilities.RemoveAll(capability => capability.Kind == EraWeaponCapabilityKind.Hitscan);
            framework.Capabilities.Add(new() { Kind = EraWeaponCapabilityKind.Projectile, Projectile = new() { MaximumDistance = 30 } });
            framework.SupplementaryStats.Add(stat);
            catalog.Templates[0].BaseStats.Add(new() { Stat = stat, Value = 100 });
        }
        else if (stat == EraWeaponStat.ChargeTime)
        {
            framework.Capabilities.Add(new() { Kind = EraWeaponCapabilityKind.Charge, Charge = new() });
            framework.Capabilities.Single(capability => capability.Kind == EraWeaponCapabilityKind.Firing).Firing!.Mode = EraWeaponFireMode.Charged;
            framework.SupplementaryStats.Add(stat);
            catalog.Templates[0].BaseStats.Add(new() { Stat = stat, Value = 1 });
        }
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
        var entry = catalog.Templates[0].BaseStats.Single(value => value.Stat == stat);
        entry.EffectAtZero = 1;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        entry.EffectAtZero = null;
        entry.EffectAtHundred = 1;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
        entry.EffectAtZero = 1;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Theory]
    [InlineData(EraWeaponStat.Stability, 181, 0)]
    [InlineData(EraWeaponStat.Stability, 1, -1)]
    [InlineData(EraWeaponStat.Stability, 1, 2)]
    [InlineData(EraWeaponStat.Handling, 3601, 1)]
    [InlineData(EraWeaponStat.Handling, 1, 0)]
    [InlineData(EraWeaponStat.Handling, 1, 2)]
    [InlineData(EraWeaponStat.Handling, double.NaN, 1)]
    [InlineData(EraWeaponStat.Handling, 1, double.PositiveInfinity)]
    public void Should_validate_effect_units_order_and_finite_bounds(EraWeaponStat stat, double zero, double hundred)
    {
        var catalog = EraWeaponFixture.Catalog();
        var entry = catalog.Templates[0].BaseStats.Single(value => value.Stat == stat);
        entry.EffectAtZero = zero;
        entry.EffectAtHundred = hundred;
        Should.Throw<ArgumentException>(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Theory]
    [InlineData(EraWeaponStat.Accuracy, 0)]
    [InlineData(EraWeaponStat.Stability, 180)]
    [InlineData(EraWeaponStat.Handling, 3600)]
    [InlineData(EraWeaponStat.Handling, .001)]
    public void Should_allow_equal_endpoints_at_valid_effect_boundaries(EraWeaponStat stat, double effect)
    {
        var catalog = EraWeaponFixture.Catalog();
        var entry = catalog.Templates[0].BaseStats.Single(value => value.Stat == stat);
        entry.EffectAtZero = effect;
        entry.EffectAtHundred = effect;
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(catalog));
    }

    [Fact]
    public void Should_preserve_independent_effect_endpoints_for_templates_sharing_a_framework()
    {
        var catalog = EraWeaponFixture.Catalog();
        var copy = JsonSerializer.Deserialize<EraWeaponTemplateDto>(JsonSerializer.Serialize(catalog.Templates[0]))!;
        copy.Id = Guid.NewGuid();
        var handling = copy.BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling);
        handling.EffectAtZero = 2;
        handling.EffectAtHundred = .5;
        catalog.Templates.Add(copy);
        var restored = JsonSerializer.Deserialize<EraWeaponCatalogDto>(JsonSerializer.Serialize(catalog))!;
        Should.NotThrow(() => EraWeaponConfigurationValidator.Validate(restored));
        restored.Templates.Select(template => template.FrameworkId).Distinct().Count().ShouldBe(1);
        restored.Templates[0].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtZero.ShouldBe(1);
        restored.Templates[1].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtZero.ShouldBe(2);
        restored.Templates[1].BaseStats.Single(entry => entry.Stat == EraWeaponStat.Handling).EffectAtHundred.ShouldBe(.5);
        var payload = JsonSerializer.SerializeToElement(handling);
        payload.EnumerateObject().Select(property => property.Name).Order().ShouldBe(new[] { "EffectAtHundred", "EffectAtZero", "Stat", "Value" });
    }

}
