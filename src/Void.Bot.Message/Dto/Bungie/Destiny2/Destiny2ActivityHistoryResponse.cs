namespace Void.Bot.Message.Dto.Bungie.Destiny2;

/// <summary>
/// Bungie API response for character activity history.
/// Used for historical season playtime aggregation.
/// </summary>
public class Destiny2ActivityHistoryResponse
{
    public int ErrorCode { get; set; }
    public string? Message { get; set; }
    public Destiny2ActivityHistoryData? Response { get; set; }
}

public class Destiny2ActivityHistoryData
{
    public List<Destiny2Activity>? Activities { get; set; }
}

public class Destiny2Activity
{
    public string? Period { get; set; }
    public uint? ActivityHash { get; set; }
    public long? Duration { get; set; }
}
