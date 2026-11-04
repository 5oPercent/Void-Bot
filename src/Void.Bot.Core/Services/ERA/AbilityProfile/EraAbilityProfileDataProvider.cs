using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.AbilityProfile;

public interface IEraAbilityProfileDataProvider : IScope
{
    Task<EraCharacterResonanceProfile?> GetProfileAsync(Guid characterId, CancellationToken cancellationToken);
    Task<List<string>> GetLearnedResonancesAsync(Guid characterId, CancellationToken cancellationToken);
    Task CreateProfileAsync(EraCharacterResonanceProfile profile, CancellationToken cancellationToken);
    Task GrantResonanceAsync(EraCharacterLearnedResonance skill, CancellationToken cancellationToken);
    void UpdateProfile(EraCharacterResonanceProfile profile);
}

/// <summary>只访问角色配装与学习记录，不访问任何配置定义表。</summary>
public sealed class EraAbilityProfileDataProvider(IRepository<EraCharacterResonanceProfile> profiles,
    IRepository<EraCharacterLearnedResonance> learned) : IEraAbilityProfileDataProvider
{
    public Task<EraCharacterResonanceProfile?> GetProfileAsync(Guid characterId, CancellationToken cancellationToken) =>
        profiles.QueryNoTracking().SingleOrDefaultAsync(profile => profile.CharacterId == characterId, cancellationToken);

    public Task<List<string>> GetLearnedResonancesAsync(Guid characterId, CancellationToken cancellationToken) =>
        learned.QueryNoTracking().Where(skill => skill.CharacterId == characterId).OrderBy(skill => skill.SkillId).Select(skill => skill.SkillId).ToListAsync(cancellationToken);

    public Task CreateProfileAsync(EraCharacterResonanceProfile profile, CancellationToken cancellationToken) => profiles.InsertAsync(profile, cancellationToken);
    public Task GrantResonanceAsync(EraCharacterLearnedResonance skill, CancellationToken cancellationToken) => learned.InsertAsync(skill, cancellationToken);
    public void UpdateProfile(EraCharacterResonanceProfile profile) => profiles.Update(profile);
}
