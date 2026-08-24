namespace Void.Bot.Message.Dto.Email;

public sealed class EmailMessageDto
{
    public required string To { get; init; }

    public required string Subject { get; init; }

    public string TextContent { get; init; } = string.Empty;

    public string HtmlContent { get; init; } = string.Empty;

    public string? ReplyTo { get; init; }
}
