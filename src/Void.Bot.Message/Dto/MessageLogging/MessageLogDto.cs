namespace Void.Bot.Message.Dto.MessageLogging;

public class MessageLogDto
{
    public Guid Id { get; set; }

    public string MessageType { get; set; }
    
    public string ResultType { get; set; }
    
    public string MessageJson { get; set; }
    
    public string ResultJson { get; set; }
    
    public Guid? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; }
}