namespace Void.Bot.Message.Dto.ERA;

public sealed class EraWorldAbilityProfilesDto
{
    public Guid WorldInstanceId { get; set; }
    public long AuthorityEpoch { get; set; }
    public List<EraWorldMemberAbilityProfileDto> Members { get; set; } = new();
}
public sealed class EraWorldMemberAbilityProfileDto
{
    public Guid MemberId { get; set; }
    public Guid CharacterId { get; set; }
    public CharacterResonanceProfile Profile { get; set; } = new();
    public List<string> LearnedSkills { get; set; } = new();
}
