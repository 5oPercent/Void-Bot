using Autofac;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

public partial class EraIntegrationFixture
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Independent_repository_coordination_should_commit_only_after_successful_external_action(bool fail)
    {
        await SeedAsync(0);
        var teamId = Guid.NewGuid();
        var repository = _root.Resolve<IRepository>();
        var statements = new FormattableString[]
        {
            $"INSERT INTO era_world_team_state (team_id, active_world_id) VALUES ({teamId.ToString("D")}, NULL) ON DUPLICATE KEY UPDATE team_id = team_id",
            $"SELECT team_id FROM era_world_team_state WHERE team_id = {teamId.ToString("D")} FOR UPDATE"
        };
        var invoked = false;
        async Task<int> Action()
        {
            invoked = true;
            // 新连接不能看见尚未提交的协调行，回调发生在持锁事务内部。
            await ReadDatabaseAsync(async context =>
                (await context.Set<EraWorldTeamState>().AnyAsync(row => row.TeamId == teamId)).ShouldBeFalse());
            if (fail) throw new InvalidOperationException("Simulated external storage failure.");
            return 42;
        }
        if (fail)
            await Should.ThrowAsync<InvalidOperationException>(() => repository.ExecuteInIndependentTransactionAsync(statements, Action, CancellationToken.None));
        else
            (await repository.ExecuteInIndependentTransactionAsync(statements, Action, CancellationToken.None)).ShouldBe(42);
        invoked.ShouldBeTrue();
        await ReadDatabaseAsync(async context =>
            (await context.Set<EraWorldTeamState>().AnyAsync(row => row.TeamId == teamId)).ShouldBe(!fail));
    }
}
