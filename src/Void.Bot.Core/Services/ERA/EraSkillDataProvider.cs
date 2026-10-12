using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraSkillDataProvider : IScope
{
    Task<EraSkillConfiguration?> GetConfigurationAsync(bool forUpdate, CancellationToken cancellationToken);

    Task<EraPlayerSkillProfile?> GetProfileAsync(Guid playerId, CancellationToken cancellationToken);

    Task<List<string>> GetLearnedSkillsAsync(Guid playerId, CancellationToken cancellationToken);

    Task CreateProfileAsync(EraPlayerSkillProfile profile, CancellationToken cancellationToken);

    Task GrantSkillAsync(EraPlayerLearnedSkill skill, CancellationToken cancellationToken);

    void UpdateProfile(EraPlayerSkillProfile profile);
}

/// <summary>
/// 技能发布、配装档案和学习记录的数据访问，不提交事务或实施装配业务规则。
/// </summary>
public class EraSkillDataProvider : IEraSkillDataProvider
{
    // 对应 DbUp 的固定种子行，防止空表锁无法保护首次发布。
    private static readonly Guid ConfigurationId = Guid.Parse("20000000-0000-0000-0000-000000000001");
    private readonly VoidBotDbContext _dbContext;

    public EraSkillDataProvider(VoidBotDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<EraSkillConfiguration?> GetConfigurationAsync(
        bool forUpdate,
        CancellationToken cancellationToken)
    {
        if (!forUpdate)
        {
            return await _dbContext.Set<EraSkillConfiguration>()
                .AsNoTracking()
                .SingleOrDefaultAsync(configuration => configuration.Id == ConfigurationId, cancellationToken)
                .ConfigureAwait(false);
        }

        // 不在带锁 SQL 上组合 LIMIT／子查询，保证锁的是发布种子行本身。
        var configurations = await _dbContext.Set<EraSkillConfiguration>()
            .FromSqlInterpolated($"SELECT * FROM era_skill_configuration WHERE id = {ConfigurationId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return configurations.SingleOrDefault();
    }

    public async Task<EraPlayerSkillProfile?> GetProfileAsync(
        Guid playerId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<EraPlayerSkillProfile>()
            .AsNoTracking()
            .SingleOrDefaultAsync(profile => profile.PlayerId == playerId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<string>> GetLearnedSkillsAsync(
        Guid playerId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Set<EraPlayerLearnedSkill>()
            .AsNoTracking()
            .Where(skill => skill.PlayerId == playerId)
            .OrderBy(skill => skill.SkillId)
            .Select(skill => skill.SkillId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateProfileAsync(
        EraPlayerSkillProfile profile,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraPlayerSkillProfile>()
            .AddAsync(profile, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task GrantSkillAsync(
        EraPlayerLearnedSkill skill,
        CancellationToken cancellationToken)
    {
        await _dbContext.Set<EraPlayerLearnedSkill>()
            .AddAsync(skill, cancellationToken)
            .ConfigureAwait(false);
    }

    public void UpdateProfile(EraPlayerSkillProfile profile)
    {
        _dbContext.Set<EraPlayerSkillProfile>().Update(profile);
    }
}
