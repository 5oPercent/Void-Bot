using Shouldly;
using System.Text.Json;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class BuffDefinitionValidatorFixture
{
    [Fact]
    public void EffectContract_UsesIdAndRejectsRetiredPresentationKey()
    {
        var definition = new BuffDefinitionDto { Id = "restore.health", Kind = BuffKind.Instant,
            Effects = new() { new() { Kind = BuffEffectKind.Heal, Amount = 25 } } };
        var json = JsonSerializer.Serialize(definition);
        json.ShouldNotContain("PresentationKey");
        JsonSerializer.Deserialize<BuffDefinitionDto>(json)!.Id.ShouldBe(definition.Id);
        var legacy = json.Insert(1, "\"PresentationKey\":\"old.presentation\",");
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<BuffDefinitionDto>(legacy));
    }

    [Fact]
    public void Validate_ItemHealingBuffDoesNotRequireResonanceConfiguration()
    {
        var definition = new BuffDefinitionDto()
        {
            Id = "item.medkit.heal",
            Kind = BuffKind.Instant,
            Effects = new List<BuffEffectDto>()
            {
                new BuffEffectDto()
                {
                    Kind = BuffEffectKind.Heal,
                    Amount = 25
                }
            }
        };

        Should.NotThrow(() => BuffDefinitionValidator.Validate(definition));
    }

    [Fact]
    public void Validate_PeriodicBuffRejectsZeroInterval()
    {
        var definition = new BuffDefinitionDto()
        {
            Id = "environment.damage",
            Kind = BuffKind.Periodic,
            IntervalSeconds = 0,
            Effects = new List<BuffEffectDto>()
            {
                new BuffEffectDto()
                {
                    Kind = BuffEffectKind.Damage,
                    Amount = 5
                }
            }
        };

        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(definition));
    }

    [Fact]
    public void Validate_InstantBuffRejectsPersistentAttributeModifier()
    {
        var definition = new BuffDefinitionDto()
        {
            Id = "equipment.strength",
            Kind = BuffKind.Instant,
            Effects = new List<BuffEffectDto>()
            {
                new BuffEffectDto()
                {
                    Kind = BuffEffectKind.Modifier,
                    Stat = 0,
                    Operation = 0,
                    Amount = 10
                }
            }
        };

        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(definition));
    }
    [Theory]
    [InlineData(-1)]
    [InlineData(.001)]
    [InlineData(1)]
    public void Periodic_effects_allow_mixed_modifiers_damage_healing_and_protection(double interval)
    {
        var definition = new BuffDefinitionDto { Id = "mixed", Kind = BuffKind.Periodic, IntervalSeconds = interval, DurationSeconds = 5,
            Effects = new() {
                new() { Kind = BuffEffectKind.Modifier, Stat = 5, Amount = -.2, Operation = 1 },
                new() { Kind = BuffEffectKind.Damage, Amount = 10 },
                new() { Kind = BuffEffectKind.Heal, Amount = 2 },
                new() { Kind = BuffEffectKind.RestoreProtection, Amount = 3 } } };
        Should.NotThrow(() => BuffDefinitionValidator.Validate(definition));
    }

    [Theory]
    [InlineData(-2)]
    [InlineData(-.5)]
    [InlineData(0)]
    [InlineData(.0001)]
    [InlineData(86401)]
    public void Periodic_effects_reject_other_intervals(double interval)
    {
        var definition = new BuffDefinitionDto { Id = "invalid", Kind = BuffKind.Periodic, IntervalSeconds = interval,
            Effects = new() { new() { Kind = BuffEffectKind.Damage, Amount = 1 } } };
        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(definition));
    }

    [Fact]
    public void Retired_persistent_kind_is_not_reinterpreted_as_periodic()
    {
        var definition = new BuffDefinitionDto { Id = "retired", Kind = (BuffKind)1,
            Effects = new() { new() { Kind = BuffEffectKind.Modifier, Stat = 5, Amount = 1 } } };
        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(definition));
    }

}
