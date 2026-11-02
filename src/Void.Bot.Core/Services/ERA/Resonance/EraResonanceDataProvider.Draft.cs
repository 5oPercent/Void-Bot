using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceDataProvider
{
    Task<EraResonanceDraftWorkspace?> GetDraftWorkspaceAsync(bool forUpdate, CancellationToken cancellationToken);
    Task<List<EraResonanceDraftRecord>> GetDraftRecordsAsync(bool forUpdate, CancellationToken cancellationToken);
    Task AddDraftRecordAsync(EraResonanceDraftRecord record, CancellationToken cancellationToken);
    Task<EraResonancePublication?> GetPublicationAsync(long version, CancellationToken cancellationToken);
    Task<List<EraResonancePublication>> GetPublicationsAsync(int skip, int take, CancellationToken cancellationToken);
    Task<int> CountPublicationsAsync(CancellationToken cancellationToken);
    Task AddPublicationAsync(EraResonancePublication publication, CancellationToken cancellationToken);
    Task<EraResonancePublishReceipt?> GetPublishReceiptAsync(Guid requestId, CancellationToken cancellationToken);
    Task AddPublishReceiptAsync(EraResonancePublishReceipt receipt, CancellationToken cancellationToken);
}

public partial class EraResonanceDataProvider
{
    public static readonly Guid DraftWorkspaceId = Guid.Parse("20000000-0000-0000-0000-000000000002");
    public async Task<EraResonanceDraftWorkspace?> GetDraftWorkspaceAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        if (!forUpdate)
        {
            return await _eraSkillDraftWorkspaceRepository.QueryNoTracking().SingleOrDefaultAsync(w => w.Id == DraftWorkspaceId, cancellationToken).ConfigureAwait(false);
        }

        var rows = await _eraSkillDraftWorkspaceRepository.QuerySql($"SELECT * FROM era_skill_draft_workspace WHERE id = {DraftWorkspaceId.ToString()} FOR UPDATE").ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public Task<List<EraResonanceDraftRecord>> GetDraftRecordsAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        var query = _eraSkillDraftRecordRepository.Query().Where(r => r.WorkspaceId == DraftWorkspaceId);
        return (forUpdate ? query : query.AsNoTracking()).ToListAsync(cancellationToken);
    }

    public async Task AddDraftRecordAsync(EraResonanceDraftRecord record, CancellationToken cancellationToken)
    {
        await _eraSkillDraftRecordRepository.InsertAsync(record, cancellationToken).ConfigureAwait(false);
    }

    public Task<EraResonancePublication?> GetPublicationAsync(long version, CancellationToken cancellationToken)
    {
        return _eraSkillPublicationRepository.QueryNoTracking().SingleOrDefaultAsync(p => p.Version == version, cancellationToken);
    }

    public Task<List<EraResonancePublication>> GetPublicationsAsync(int skip, int take, CancellationToken cancellationToken)
    {
        return _eraSkillPublicationRepository.QueryNoTracking().OrderByDescending(p => p.Version).Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public Task<int> CountPublicationsAsync(CancellationToken cancellationToken)
    {
        return _eraSkillPublicationRepository.Query().CountAsync(cancellationToken);
    }

    public async Task AddPublicationAsync(EraResonancePublication publication, CancellationToken cancellationToken)
    {
        await _eraSkillPublicationRepository.InsertAsync(publication, cancellationToken).ConfigureAwait(false);
    }

    public Task<EraResonancePublishReceipt?> GetPublishReceiptAsync(Guid requestId, CancellationToken cancellationToken)
    {
        return _eraSkillPublishReceiptRepository.QueryNoTracking().SingleOrDefaultAsync(r => r.Id == requestId, cancellationToken);
    }

    public async Task AddPublishReceiptAsync(EraResonancePublishReceipt receipt, CancellationToken cancellationToken)
    {
        await _eraSkillPublishReceiptRepository.InsertAsync(receipt, cancellationToken).ConfigureAwait(false);
    }
}
