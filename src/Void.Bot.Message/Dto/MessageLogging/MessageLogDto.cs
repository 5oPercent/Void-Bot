namespace Void.Bot.Message.Dto.MessageLogging;

public class MessageLogDto
{
    public Guid Id { get; set; }

    public string MessageType { get; set; } = string.Empty;
    
    public string ResultType { get; set; } = string.Empty;
    
    public string MessageJson { get; set; } = string.Empty;
    
    public string ResultJson { get; set; } = string.Empty;
    
    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}