using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Resonance;

public partial interface IEraResonanceDataProvider : IScope
{
    Task<EraResonanceConfiguration?> GetConfigurationAsync(bool forUpdate, CancellationToken cancellationToken);

    Task<EraCharacterResonanceProfile?> GetProfileAsync(Guid characterId, CancellationToken cancellationToken);

    Task<List<string>> GetLearnedResonancesAsync(Guid characterId, CancellationToken cancellationToken);

    Task CreateProfileAsync(EraCharacterResonanceProfile profile, CancellationToken cancellationToken);

    Task GrantResonanceAsync(EraCharacterLearnedResonance skill, CancellationToken cancellationToken);

    void UpdateProfile(EraCharacterResonanceProfile profile);
}

/// <summary>
/// 谐振技能发布、配装档案和学习记录的数据访问，不提交事务或实施装配业务规则。
/// </summary>
public partial class EraResonanceDataProvider : IEraResonanceDataProvider
{
    // 对应 DbUp 的固定种子行，防止空表锁无法保护首次发布。
    private static readonly Guid ConfigurationId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private readonly IRepository<EraResonanceConfiguration> _eraSkillConfigurationRepository;
    private readonly IRepository<EraCharacterResonanceProfile> _eraCharacterResonanceProfileRepository;
    private readonly IRepository<EraCharacterLearnedResonance> _eraCharacterLearnedResonanceRepository;
    private readonly IRepository<EraResonanceDraftWorkspace> _eraSkillDraftWorkspaceRepository;
    private readonly IRepository<EraResonanceDraftRecord> _eraSkillDraftRecordRepository;
    private readonly IRepository<EraResonancePublication> _eraSkillPublicationRepository;
    private readonly IRepository<EraResonancePublishReceipt> _eraSkillPublishReceiptRepository;

    public EraResonanceDataProvider(
        IRepository<EraResonanceConfiguration> eraSkillConfigurationRepository,
        IRepository<EraCharacterResonanceProfile> eraCharacterResonanceProfileRepository,
        IRepository<EraCharacterLearnedResonance> eraCharacterLearnedResonanceRepository,
        IRepository<EraResonanceDraftWorkspace> eraSkillDraftWorkspaceRepository,
        IRepository<EraResonanceDraftRecord> eraSkillDraftRecordRepository,
        IRepository<EraResonancePublication> eraSkillPublicationRepository,
        IRepository<EraResonancePublishReceipt> eraSkillPublishReceiptRepository)
    {
        _eraSkillConfigurationRepository = eraSkillConfigurationRepository;
        _eraCharacterResonanceProfileRepository = eraCharacterResonanceProfileRepository;
        _eraCharacterLearnedResonanceRepository = eraCharacterLearnedResonanceRepository;
        _eraSkillDraftWorkspaceRepository = eraSkillDraftWorkspaceRepository;
        _eraSkillDraftRecordRepository = eraSkillDraftRecordRepository;
        _eraSkillPublicationRepository = eraSkillPublicationRepository;
        _eraSkillPublishReceiptRepository = eraSkillPublishReceiptRepository;
    }

    public async Task<EraResonanceConfiguration?> GetConfigurationAsync(
        bool forUpdate,
        CancellationToken cancellationToken)
    {
        if (!forUpdate)
        {
            return await _eraSkillConfigurationRepository.QueryNoTracking()
                .SingleOrDefaultAsync(configuration => configuration.Id == ConfigurationId, cancellationToken)
                .ConfigureAwait(false);
        }

        // 不在带锁 SQL 上组合 LIMIT／子查询，保证锁的是发布种子行本身。
        var configurations = await _eraSkillConfigurationRepository.QuerySql($"SELECT * FROM era_skill_configuration WHERE id = {ConfigurationId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return configurations.SingleOrDefault();
    }

    public async Task<EraCharacterResonanceProfile?> GetProfileAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterResonanceProfileRepository.QueryNoTracking()
            .SingleOrDefaultAsync(profile => profile.CharacterId == characterId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<string>> GetLearnedResonancesAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterLearnedResonanceRepository.QueryNoTracking()
            .Where(skill => skill.CharacterId == characterId)
            .OrderBy(skill => skill.SkillId)
            .Select(skill => skill.SkillId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateProfileAsync(
        EraCharacterResonanceProfile profile,
        CancellationToken cancellationToken)
    {
        await _eraCharacterResonanceProfileRepository.InsertAsync(profile, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task GrantResonanceAsync(
        EraCharacterLearnedResonance skill,
        CancellationToken cancellationToken)
    {
        await _eraCharacterLearnedResonanceRepository.InsertAsync(skill, cancellationToken)
            .ConfigureAwait(false);
    }

    public void UpdateProfile(EraCharacterResonanceProfile profile)
    {
        _eraCharacterResonanceProfileRepository.Update(profile);
    }
}
