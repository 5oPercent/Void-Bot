using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class DiscordGuildManagementCheckFailedException : BusinessException
{
    public DiscordGuildManagementCheckFailedException(HttpStatusCode code, string message)
        : base(code, string.IsNullOrWhiteSpace(message)
            ? "Failed to verify Discord guild management permission."
            : message)
    {
    }
}
