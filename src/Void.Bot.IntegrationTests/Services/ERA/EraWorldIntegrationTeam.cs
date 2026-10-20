using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.ERA;

namespace Void.Bot.IntegrationTest.Services.ERA;

/// <summary>仅删除本例创建的随机队伍，不清空共享 Redis 数据库。</summary>
internal sealed class EraWorldIntegrationTeam : IAsyncDisposable
{
    public EraTeam Team { get; }
    public IEraTeamDataProvider Provider { get; }

    public EraWorldIntegrationTeam(EraTeam team, IEraTeamDataProvider provider)
    {
        Team = team;
        Provider = provider;
    }

    public async ValueTask DisposeAsync()
    {
        await Provider.ChangeAsync(Team.TeamId, team => new EraTeamChange<bool>
        {
            Changed = team != null, Team = null, Result = true
        }, CancellationToken.None);
    }
}
