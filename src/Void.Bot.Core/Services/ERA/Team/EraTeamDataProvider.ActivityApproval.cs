using StackExchange.Redis;

namespace Void.Bot.Core.Services.ERA.Team;

public interface IEraActivityApprovalStore : ISingleton
{
    Task<bool> StoreAsync(string key, string value, CancellationToken cancellationToken);
    Task<string?> ReadAsync(string key, CancellationToken cancellationToken);
    Task<bool> ConsumeAsync(string key, CancellationToken cancellationToken);
}

public partial class EraTeamDataProvider : IEraActivityApprovalStore
{
    private static RedisKey ActivityApprovalKey(string key) => "era:world:modular-activity:v1:" + key;

    async Task<bool> IEraActivityApprovalStore.StoreAsync(string key, string value, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        return await database.StringSetAsync(ActivityApprovalKey(key), value, TimeSpan.FromSeconds(20), when: When.NotExists)
            .WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    async Task<string?> IEraActivityApprovalStore.ReadAsync(string key, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        var value = await database.StringGetAsync(ActivityApprovalKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
        return value.IsNull ? null : value.ToString();
    }

    async Task<bool> IEraActivityApprovalStore.ConsumeAsync(string key, CancellationToken cancellationToken)
    {
        var database = await GetDatabaseAsync(cancellationToken).ConfigureAwait(false);
        // 审批键不可覆盖；只允许成功删除该键的领取请求得到配置。
        return await database.KeyDeleteAsync(ActivityApprovalKey(key)).WaitAsync(cancellationToken).ConfigureAwait(false);
    }
}
