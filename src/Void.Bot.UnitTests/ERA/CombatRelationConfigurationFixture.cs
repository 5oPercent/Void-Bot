using Shouldly;
using Void.Bot.Message.Enum.ERA;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.UnitTest.ERA;

public sealed class CombatRelationConfigurationFixture
{
    private static CombatRelationConfigurationDto Catalog() => new()
    {
        Species = new() { new() { Id = "construct", Name = "构造体" } },
        Factions = new() { new() { Id = "players", Name = "玩家方" }, new() { Id = "hostile", Name = "敌对阵营" } },
        FactionRules = new() { new() { ObserverId = "players", TargetId = "hostile", Relation = 4 } }
    };

    [Fact]
    public void Bidirectional_rules_reserve_both_directions_and_asymmetric_rules_remain_valid()
    {
        var value = Catalog();
        Should.NotThrow(() => CombatRelationConfigurationValidator.Validate(value));
        value.FactionRules.Add(new() { ObserverId = "hostile", TargetId = "players", Relation = 3, Bidirectional = false });
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.Validate(value));
        value.FactionRules[0].Bidirectional = false;
        Should.NotThrow(() => CombatRelationConfigurationValidator.Validate(value));
        value.FactionRules[0].TargetId = "missing";
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.Validate(value));
    }

    [Fact]
    public void Duplicate_identity_keys_and_runtime_only_relation_values_are_rejected()
    {
        var value = Catalog();
        value.Species.Add(new() { Id = "construct", Name = "重复" });
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.Validate(value));
        value.Species.RemoveAt(1);
        value.FactionRules[0].Relation = 1;
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.Validate(value));
        value.FactionRules[0].Relation = 4;
        value.Species[0].Id = "invalid:key";
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.Validate(value));
    }

    [Fact]
    public void Nested_identity_conditions_and_effect_overrides_require_published_keys()
    {
        var condition = new ResonanceEntityConditionDto { Kind = ResonanceConditionKind.AllOf, Children = new() { new() { Kind = ResonanceConditionKind.Species, IdentityId = "construct" }, new() { Kind = ResonanceConditionKind.Faction, IdentityId = "players" } } };
        var configuration = new ResonanceConfiguration { CombatRelations = Catalog(), Skills = new() { new() { Id = "skill", Variants = new() { new() { Condition = condition } } } } };
        Should.NotThrow(() => CombatRelationConfigurationValidator.ValidateConditions(configuration));
        configuration.CombatRelations.Species.Clear();
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.ValidateConditions(configuration));
        condition.Children[0] = new() { Kind = ResonanceConditionKind.Relation, Relation = 3 };
        Should.NotThrow(() => CombatRelationConfigurationValidator.ValidateConditions(configuration));
        var variant = configuration.Skills[0].Variants[0];
        variant.Effects.Add(new() { EffectId = "heal", Condition = new() { Kind = ResonanceConditionKind.Species, IdentityId = "missing" } });
        Should.Throw<ArgumentException>(() => CombatRelationConfigurationValidator.ValidateConditions(configuration));
        variant.Effects[0].Condition = new() { Kind = ResonanceConditionKind.Relation, Relation = 2 };
        Should.NotThrow(() => CombatRelationConfigurationValidator.ValidateConditions(configuration));
    }
}
