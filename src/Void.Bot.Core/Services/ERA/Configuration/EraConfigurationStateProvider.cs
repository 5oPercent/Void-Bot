using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Services.Identity;

namespace Void.Bot.Core.Services.ERA.Configuration;

public interface IEraConfigurationStateProvider : IScope
{
    Task<EraDataEditState?> GetAsync(bool forUpdate, CancellationToken cancellationToken);
    void Touch(EraDataEditState state);
}

/// <summary>只协调跨表引用写入和一致性读取，不存储任何业务定义。</summary>
public sealed class EraConfigurationStateProvider(IRepository<EraDataEditState> repository, ICurrentUser currentUser) : IEraConfigurationStateProvider
{
    public static readonly Guid DataStateId = Guid.Parse("20000000-0000-0000-0000-000000000002");

    public async Task<EraDataEditState?> GetAsync(bool forUpdate, CancellationToken cancellationToken) => forUpdate
        ? (await repository.QuerySql($"SELECT * FROM era_data_edit_state WHERE id = {DataStateId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false)).SingleOrDefault()
        : await repository.QueryNoTracking().SingleOrDefaultAsync(state => state.Id == DataStateId, cancellationToken).ConfigureAwait(false);

    public void Touch(EraDataEditState state)
    {
        state.Revision = checked(state.Revision + 1);
        state.LastModifiedBy = currentUser.Id;
        state.UpdatedAt = DateTimeOffset.UtcNow;
    }
}
