using Shouldly;
using System.Text.Json;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class AbilityExecutionContractFixture
{
    [Fact]
    public void Fixed_variants_validate_unlimited_distance_delay_and_carrier_lifetime()
    {
        var variant = new ResonanceVariant { MaximumDistance = -1, ExecutionDelaySeconds = 1 };
        Should.NotThrow(() => AbilityExecutionContract.Variant(variant));
        variant.MaximumDistance = -2;
        Should.Throw<ArgumentException>(() => AbilityExecutionContract.Variant(variant));
        variant.MaximumDistance = -1; variant.CarrierKind = ResonanceCarrierKind.Area;
        Should.Throw<ArgumentException>(() => AbilityExecutionContract.Variant(variant));
        variant.DurationSeconds = 4; variant.TriggerIntervalSeconds = 1;
        Should.NotThrow(() => AbilityExecutionContract.Variant(variant));
        variant.ExecutionDelaySeconds = 5; // A carrier can expire before its first behavior.
        Should.NotThrow(() => AbilityExecutionContract.Variant(variant));
        foreach (var interval in new[] { 0, .0001, double.PositiveInfinity })
        {
            variant.TriggerIntervalSeconds = interval;
            Should.Throw<ArgumentException>(() => AbilityExecutionContract.Variant(variant));
        }
    }

    [Fact]
    public void Effect_overrides_are_independent_and_effect_lifecycle_is_not_duplicated()
    {
        var variant = new ResonanceVariant { CarrierKind = ResonanceCarrierKind.Area, DurationSeconds = 4, TriggerIntervalSeconds = 1,
            Condition = new() { Kind = ResonanceConditionKind.Relation, Relation = 4 },
            Effects = new() { new() { EffectId = "damage" }, new() { EffectId = "heal", Condition = new() { Kind = ResonanceConditionKind.Relation, Relation = 2 } } } };
        Should.NotThrow(() => AbilityExecutionContract.Variant(variant));
        variant.Condition.Relation.ShouldBe(4); variant.Effects[1].Condition!.Relation.ShouldBe(2);
        variant.Effects[0].Condition.ShouldBeNull();
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<ResonanceVariant>("{\"CarrierId\":\"self\"}"));
        Should.Throw<JsonException>(() => JsonSerializer.Deserialize<EraDataSettings>("{\"EditorDefinitions\":[]}"));
    }

    [Fact]
    public void Conditions_are_fixed_nodes_and_invalid_or_excessive_trees_are_rejected()
    {
        Should.Throw<ArgumentException>(() => AbilityExecutionContract.Condition(new() { Kind = (ResonanceConditionKind)99 }));
        var condition = new ResonanceEntityConditionDto();
        for (int i = 0; i < 8; i++) condition = new() { Kind = ResonanceConditionKind.Not, Children = new() { condition } };
        Should.Throw<ArgumentException>(() => AbilityExecutionContract.Condition(condition));
        Should.Throw<ArgumentException>(() => AbilityExecutionContract.Condition(new() { Kind = ResonanceConditionKind.Species }));
    }

    [Fact]
    public void Additional_six_dimensional_attributes_and_lifecycle_rules_share_the_formal_contract()
    {
        var buff = new BuffDefinitionDto { Id = "efficiency", Kind = Void.Bot.Message.Enum.ERA.BuffKind.Periodic,
            DurationSeconds = 3, Effects = new() { new() { Stat = 8, Amount = 5 } } };
        Should.NotThrow(() => BuffDefinitionValidator.Validate(buff));
        buff.Effects[0].Operation = 1;
        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(buff));
        buff.Effects[0].Operation = 0; buff.Lifecycle.DecayMode = 2; buff.DurationSeconds = 0;
        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(buff));
        buff.DurationSeconds = 3; buff.Lifecycle.CountEvents = true;
        Should.Throw<ArgumentException>(() => BuffDefinitionValidator.Validate(buff));
        buff.StackRule = 2;
        Should.NotThrow(() => BuffDefinitionValidator.Validate(buff));
    }
}
