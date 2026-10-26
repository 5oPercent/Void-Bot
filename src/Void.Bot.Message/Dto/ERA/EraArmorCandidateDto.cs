namespace Void.Bot.Message.Dto.ERA;

public sealed class EraArmorCandidateDto
{
    public Guid CandidateId { get; set; }
    public Guid ModDefinitionId { get; set; }
    public int DisplayOrder { get; set; }
}
