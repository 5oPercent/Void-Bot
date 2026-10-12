using Shouldly;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class BuffDefinitionValidatorFixture
{
    [Fact]
    public void Validate_ItemHealingBuffDoesNotRequireSkillConfiguration()
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
}
