using System.Net;
using System.Text.Json;
using Shouldly;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA.Team;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Fact]
    [Trait("Category", "ERADropIntegration")]
    public async Task Ground_rewards_should_follow_current_effective_members_without_changing_historical_rosters()
    {
        await SeedAsync(0);
        await ConfigureRewardsAsync(source: EraRewardSourceType.Monster, delivery: EraRewardDelivery.Ground);
        await using var team = await CreateWorldTeamAsync(includeOther: true);
        var world = await ReadyDropWorldAsync(team);
        var command = GroundCommand(world, team);
        var first = await SubmitGroundAsync(command);
        first.Data!.Grants.Count.ShouldBe(2);
        await team.Provider.ChangeAsync(team.Team.TeamId, current =>
        {
            current!.Members.RemoveAll(member => member.PlayerId == _otherPlayerId);
            return new EraTeamChange<bool> { Changed = true, Team = current, Result = true };
        }, CancellationToken.None);
        var next = await SubmitGroundAsync(GroundCommand(world, team));
        next.Code.ShouldBe(HttpStatusCode.OK);
        next.Data!.Grants.Count.ShouldBe(1);
        next.Data.Grants.Single().PlayerId.ShouldBe(_playerId);
        command.RequestId = Guid.NewGuid();
        var replay = await SubmitGroundAsync(command);
        replay.IsReplay.ShouldBeTrue();
        JsonSerializer.Serialize(replay.Data).ShouldBe(JsonSerializer.Serialize(first.Data));
        // 新事件使用当前名单，已生成的私人掉落及原分享资格不会随退队而撤销。
        (await QueryDropsAsync(world.WorldInstanceId, _otherAccountId)).Data!.Drops.Count.ShouldBe(1);
    }
}
