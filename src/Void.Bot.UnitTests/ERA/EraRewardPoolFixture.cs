using Shouldly;
using Void.Bot.Core.Services.ERA.Reward;
using Void.Bot.Core.Validators.ERA;
using Void.Bot.Message.Dto.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.UnitTest.ERA;

public class EraRewardPoolFixture
{
    [Theory]
    [InlineData(0, 0, 0)]
    [InlineData(100, 99, 1)]
    [InlineData(100, 100, 0)]
    [InlineData(10000, 9999, 1)]
    public void Trigger_chance_should_have_exact_boundaries(int chance, int ticket, int expectedCount)
    {
        var node = new EraRewardNode { ChanceBasisPoints = chance, Rolls = 1 };
        var pool = Pool();
        var result = new EraRewardPoolRoller().Roll(node, pool, new TicketRandom(ticket, 0));
        result.Count.ShouldBe(expectedCount);
    }

    [Theory]
    [InlineData(0, "10000000-0000-0000-0000-000000000001")]
    [InlineData(1, "10000000-0000-0000-0000-000000000001")]
    [InlineData(2, "10000000-0000-0000-0000-000000000002")]
    [InlineData(6, "10000000-0000-0000-0000-000000000002")]
    public void Pool_weights_should_use_half_open_ticket_ranges(long ticket, string definition)
    {
        var node = new EraRewardNode { ChanceBasisPoints = 10000, Rolls = 2 };
        var result = new EraRewardPoolRoller().Roll(node, Pool(), new TicketRandom(0, ticket));
        result.Count.ShouldBe(2);
        result.ShouldAllBe(entry => entry.TemplateId == Guid.Parse(definition)); // 有放回抽取，同一项允许再次命中。
    }

    [Fact]
    public void Sources_should_share_a_pool_and_configure_delivery_independently()
    {
        var rules = new EraRewardRules
        {
            Pools = new List<EraRewardPool> { Pool() },
            Nodes = new List<EraRewardNode>
            {
                Node(EraRewardSourceType.Task, EraRewardDelivery.Inventory),
                Node(EraRewardSourceType.Monster, EraRewardDelivery.Inventory),
                Node(EraRewardSourceType.Chest, EraRewardDelivery.Ground),
                Node(EraRewardSourceType.Event, EraRewardDelivery.Ground)
            }
        };
        Should.NotThrow(() => EraRewardConfigurationValidator.Validate(rules));
        rules.Nodes[1].PoolId = "missing";
        Should.Throw<ArgumentException>(() => EraRewardConfigurationValidator.Validate(rules));
    }

    [Theory]
    [InlineData(-1, 1, 1)]
    [InlineData(10001, 1, 1)]
    [InlineData(10000, 0, 1)]
    [InlineData(10000, 1, 0)]
    public void Invalid_chance_rolls_or_weights_should_reject_configuration(int chance, int rolls, int weight)
    {
        var node = Node(EraRewardSourceType.Task, EraRewardDelivery.Inventory);
        node.ChanceBasisPoints = chance;
        node.Rolls = rolls;
        var pool = Pool();
        pool.Entries[0].Weight = weight;
        Should.Throw<ArgumentException>(() => EraRewardConfigurationValidator.Validate(new EraRewardRules
        {
            Nodes = new List<EraRewardNode> { node }, Pools = new List<EraRewardPool> { pool }
        }));
    }

    [Theory]
    [InlineData(120, 1, true)]
    [InlineData(1, 120, true)]
    [InlineData(12, 10, true)]
    [InlineData(121, 1, false)]
    [InlineData(1, 121, false)]
    [InlineData(11, 11, false)]
    public void Reward_batch_should_respect_shared_120_item_capacity(int count, int rolls, bool valid)
    {
        var node = Node(EraRewardSourceType.Task, EraRewardDelivery.Inventory);
        node.Rolls = rolls;
        var pool = Pool();
        pool.Entries[0].Count = count;
        var rules = new EraRewardRules { Nodes = new List<EraRewardNode> { node }, Pools = new List<EraRewardPool> { pool } };
        if (valid)
        {
            Should.NotThrow(() => EraRewardConfigurationValidator.Validate(rules));
        }
        else
        {
            Should.Throw<ArgumentException>(() => EraRewardConfigurationValidator.Validate(rules));
        }
    }

    private static EraRewardNode Node(EraRewardSourceType source, EraRewardDelivery delivery)
    {
        return new EraRewardNode
        {
            SourceType = source, SourceDefinitionId = "source", RewardNodeId = "node", PoolId = "pool",
            Delivery = delivery, ChanceBasisPoints = 10000, Rolls = 1
        };
    }

    private static EraRewardPool Pool()
    {
        return new EraRewardPool
        {
            Id = "pool", Entries = new List<EraRewardPoolEntry>
            {
                new EraRewardPoolEntry { TemplateId = Guid.Parse("10000000-0000-0000-0000-000000000001"), Grade = EraEquipmentGrade.Common, Count = 1, Weight = 2 },
                new EraRewardPoolEntry { TemplateId = Guid.Parse("10000000-0000-0000-0000-000000000002"), Grade = EraEquipmentGrade.Reliable, Count = 1, Weight = 5 }
            }
        };
    }

    private sealed class TicketRandom(int chanceTicket, long poolTicket) : Random
    {
        public override int Next(int maxValue)
        {
            return chanceTicket;
        }

        public override long NextInt64(long maxValue)
        {
            return poolTicket;
        }
    }
}
