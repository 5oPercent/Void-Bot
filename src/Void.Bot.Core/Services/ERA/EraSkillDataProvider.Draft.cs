using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA;

public partial interface IEraSkillDataProvider
{
    Task<EraSkillDraftWorkspace?> GetDraftWorkspaceAsync(bool forUpdate, CancellationToken cancellationToken);
    Task<List<EraSkillDraftRecord>> GetDraftRecordsAsync(bool forUpdate, CancellationToken cancellationToken);
    Task AddDraftRecordAsync(EraSkillDraftRecord record, CancellationToken cancellationToken);
    Task<EraSkillPublication?> GetPublicationAsync(long version, CancellationToken cancellationToken);
    Task<List<EraSkillPublication>> GetPublicationsAsync(int skip, int take, CancellationToken cancellationToken);
    Task<int> CountPublicationsAsync(CancellationToken cancellationToken);
    Task AddPublicationAsync(EraSkillPublication publication, CancellationToken cancellationToken);
    Task<EraSkillPublishReceipt?> GetPublishReceiptAsync(Guid requestId, CancellationToken cancellationToken);
    Task AddPublishReceiptAsync(EraSkillPublishReceipt receipt, CancellationToken cancellationToken);
}

public partial class EraSkillDataProvider
{
    public static readonly Guid DraftWorkspaceId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public async Task<EraSkillDraftWorkspace?> GetDraftWorkspaceAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        if (!forUpdate)
        {
            return await _dbContext.Set<EraSkillDraftWorkspace>().AsNoTracking().SingleOrDefaultAsync(w => w.Id == DraftWorkspaceId, cancellationToken).ConfigureAwait(false);
        }

        var rows = await _dbContext.Set<EraSkillDraftWorkspace>().FromSqlInterpolated($"SELECT * FROM era_skill_draft_workspace WHERE id = {DraftWorkspaceId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<List<EraSkillDraftRecord>> GetDraftRecordsAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        var query = _dbContext.Set<EraSkillDraftRecord>().Where(r => r.WorkspaceId == DraftWorkspaceId);
        return (forUpdate ? query : query.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public async Task AddDraftRecordAsync(EraSkillDraftRecord record, CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraSkillDraftRecord>().AddAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public Task<EraSkillPublication?> GetPublicationAsync(long version, CancellationToken cancellationToken)
    {
        return _dbContext.Set<EraSkillPublication>().AsNoTracking().SingleOrDefaultAsync(p => p.Version == version, cancellationToken);
    }

    public Task<List<EraSkillPublication>> GetPublicationsAsync(int skip, int take, CancellationToken cancellationToken)
    {
        return _dbContext.Set<EraSkillPublication>().AsNoTracking().OrderByDescending(p => p.Version).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountPublicationsAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Set<EraSkillPublication>().CountAsync(cancellationToken);
    }

    public async Task AddPublicationAsync(EraSkillPublication publication, CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraSkillPublication>().AddAsync(publication, cancellationToken).ConfigureAwait(false);
    }

    public Task<EraSkillPublishReceipt?> GetPublishReceiptAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return _dbContext.Set<EraSkillPublishReceipt>().AsNoTracking().SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken);
    }

    public async Task AddPublishReceiptAsync(EraSkillPublishReceipt receipt, CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraSkillPublishReceipt>().AddAsync(receipt, cancellationToken).ConfigureAwait(false);
    }
}
