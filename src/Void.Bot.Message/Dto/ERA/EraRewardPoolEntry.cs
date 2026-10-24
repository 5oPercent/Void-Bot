using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Message.Dto.ERA;

public sealed class EraRewardPoolEntry
{
    public Guid DefinitionId { get; set; }
    public EraEquipmentGrade Grade { get; set; }
    public int Count { get; set; }
    public int Weight { get; set; }
}
