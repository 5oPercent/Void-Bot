namespace Void.Bot.Message.Dto.ERA;

public sealed class EraRewardPool
{
    public string Id { get; set; } = string.Empty;
    public List<EraRewardPoolEntry> Entries { get; set; } = new List<EraRewardPoolEntry>();
}
