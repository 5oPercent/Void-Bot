namespace Void.Bot.Message.Dto.Bungie;

public class BungieApiResponse
{
    public int ErrorCode { get; set; }
    
    public int ThrottleSeconds { get; set; }

    public string ErrorStatus { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public object? MessageData { get; set; }
}