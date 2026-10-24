using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Skill;

public partial interface IEraSkillDataProvider : IScope
{
    Task<EraSkillConfiguration?> GetConfigurationAsync(bool forUpdate, CancellationToken cancellationToken);

    Task<EraCharacterSkillProfile?> GetProfileAsync(Guid characterId, CancellationToken cancellationToken);

    Task<List<string>> GetLearnedSkillsAsync(Guid characterId, CancellationToken cancellationToken);

    Task CreateProfileAsync(EraCharacterSkillProfile profile, CancellationToken cancellationToken);

    Task GrantSkillAsync(EraCharacterLearnedSkill skill, CancellationToken cancellationToken);

    void UpdateProfile(EraCharacterSkillProfile profile);
}

/// <summary>
/// 技能发布、配装档案和学习记录的数据访问，不提交事务或实施装配业务规则。
/// </summary>
public partial class EraSkillDataProvider : IEraSkillDataProvider
{
    // 对应 DbUp 的固定种子行，防止空表锁无法保护首次发布。
    private static readonly Guid ConfigurationId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private readonly IRepository<EraSkillConfiguration> _eraSkillConfigurationRepository;
    private readonly IRepository<EraCharacterSkillProfile> _eraCharacterSkillProfileRepository;
    private readonly IRepository<EraCharacterLearnedSkill> _eraCharacterLearnedSkillRepository;
    private readonly IRepository<EraSkillDraftWorkspace> _eraSkillDraftWorkspaceRepository;
    private readonly IRepository<EraSkillDraftRecord> _eraSkillDraftRecordRepository;
    private readonly IRepository<EraSkillPublication> _eraSkillPublicationRepository;
    private readonly IRepository<EraSkillPublishReceipt> _eraSkillPublishReceiptRepository;

    public EraSkillDataProvider(
        IRepository<EraSkillConfiguration> eraSkillConfigurationRepository,
        IRepository<EraCharacterSkillProfile> eraCharacterSkillProfileRepository,
        IRepository<EraCharacterLearnedSkill> eraCharacterLearnedSkillRepository,
        IRepository<EraSkillDraftWorkspace> eraSkillDraftWorkspaceRepository,
        IRepository<EraSkillDraftRecord> eraSkillDraftRecordRepository,
        IRepository<EraSkillPublication> eraSkillPublicationRepository,
        IRepository<EraSkillPublishReceipt> eraSkillPublishReceiptRepository)
    {
        _eraSkillConfigurationRepository = eraSkillConfigurationRepository;
        _eraCharacterSkillProfileRepository = eraCharacterSkillProfileRepository;
        _eraCharacterLearnedSkillRepository = eraCharacterLearnedSkillRepository;
        _eraSkillDraftWorkspaceRepository = eraSkillDraftWorkspaceRepository;
        _eraSkillDraftRecordRepository = eraSkillDraftRecordRepository;
        _eraSkillPublicationRepository = eraSkillPublicationRepository;
        _eraSkillPublishReceiptRepository = eraSkillPublishReceiptRepository;
    }

    public async Task<EraSkillConfiguration?> GetConfigurationAsync(
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

    public async Task<EraCharacterSkillProfile?> GetProfileAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterSkillProfileRepository.QueryNoTracking()
            .SingleOrDefaultAsync(profile => profile.CharacterId == characterId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<string>> GetLearnedSkillsAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterLearnedSkillRepository.QueryNoTracking()
            .Where(skill => skill.CharacterId == characterId)
            .OrderBy(skill => skill.SkillId)
            .Select(skill => skill.SkillId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateProfileAsync(
        EraCharacterSkillProfile profile,
        CancellationToken cancellationToken)
    {
        await _eraCharacterSkillProfileRepository.InsertAsync(profile, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task GrantSkillAsync(
        EraCharacterLearnedSkill skill,
        CancellationToken cancellationToken)
    {
        await _eraCharacterLearnedSkillRepository.InsertAsync(skill, cancellationToken)
            .ConfigureAwait(false);
    }

    public void UpdateProfile(EraCharacterSkillProfile profile)
    {
        _eraCharacterSkillProfileRepository.Update(profile);
    }
}
