namespace Void.Bot.Message.Dto.Identity;

public sealed record OAuthCallbackContext(string? Code, string State, string? Error);
