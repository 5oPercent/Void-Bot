using Shouldly;
using Void.Bot.Core.Services.ERA.Equipment;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraEquipmentRewardFixture
{
    [Theory]
    [InlineData(0, 2)]
    [InlineData(-1, 2)]
    [InlineData(121, 2)]
    [InlineData(1, 0)]
    [InlineData(1, 6)]
    public void Direct_reward_entries_should_reject_invalid_count_or_grade(int count, int grade)
    {
        new EraEquipmentRewardEntryValidator().Validate(new EraEquipmentRewardEntry
        {
            DefinitionId = Guid.NewGuid(), Count = count, Grade = (EraEquipmentGrade)grade
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Direct_ground_request_should_limit_total_count_and_reject_null_entries()
    {
        var command = new GrantEraGroundRewardCommand
        {
            WorldInstanceId = Guid.NewGuid(), RequestId = Guid.NewGuid(), CharacterId = Guid.NewGuid(), MemberId = Guid.NewGuid(), AuthorityEpoch = 1,
            Entries = new List<EraEquipmentRewardEntry>
            {
                new EraEquipmentRewardEntry { DefinitionId = Guid.NewGuid(), Count = 60 },
                new EraEquipmentRewardEntry { DefinitionId = Guid.NewGuid(), Count = 60 }
            }
        };
        var validator = new GrantEraGroundRewardCommandValidator();
        validator.Validate(command).IsValid.ShouldBeTrue();
        command.Entries[0].Count++;
        validator.Validate(command).IsValid.ShouldBeFalse();
        command.Entries[0] = null!;
        validator.Validate(command).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void Common_generation_should_consume_pool_results_or_direct_entries_without_new_definitions()
    {
        var definition = new EraArmorDefinitionDto { Id = Guid.NewGuid() };
        var manifest = new EraEquipmentManifestDto { Definitions = new List<EraArmorDefinitionDto> { definition } };
        var direct = new EraEquipmentRewardEntry { DefinitionId = definition.Id, Grade = EraEquipmentGrade.Reliable, Count = 2 };
        var generated = EraRewardEquipmentGenerator.Generate(new[] { direct }, manifest, new EraArmorGenerator(), null);
        generated.Count.ShouldBe(2);
        generated.Select(item => item.EquipmentId).Distinct().Count().ShouldBe(2);
        generated.ShouldAllBe(item => item.DefinitionId == definition.Id && item.Grade == direct.Grade && item.GenerationRuleVersion > 0);
        var pool = new EraRewardPool { Entries = new List<EraRewardPoolEntry>
        {
            new EraRewardPoolEntry { DefinitionId = definition.Id, Count = 2, Grade = direct.Grade, Weight = 1 }
        } };
        var rolled = new EraRewardPoolRoller().Roll(new EraRewardNode { ChanceBasisPoints = 10000, Rolls = 1 }, pool);
        EraRewardEquipmentGenerator.Generate(rolled, manifest, new EraArmorGenerator(), null).Count.ShouldBe(2);
        direct.DefinitionId = Guid.NewGuid();
        Should.Throw<ArgumentException>(() => EraRewardEquipmentGenerator.Generate(new[] { direct }, manifest, new EraArmorGenerator(), null));
    }
}
