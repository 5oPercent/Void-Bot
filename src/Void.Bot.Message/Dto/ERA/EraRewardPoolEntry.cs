using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

public sealed class EraRewardPoolEntry
{
    public string DefinitionId { get; set; } = string.Empty;
    public EraArmorGrade Grade { get; set; }
    public int Count { get; set; }
    public int Weight { get; set; }
}
