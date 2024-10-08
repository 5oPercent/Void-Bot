using Void.Bot.Message.Enum;

namespace Void.Bot.Message.Dto.Authorization;

public class AuthorizationDto
{
    public AppPlatformEnum Platform { get; set; }

    public string? AccountId { get; set; } = string.Empty;
}