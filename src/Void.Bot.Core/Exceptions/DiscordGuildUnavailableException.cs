using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class DiscordGuildUnavailableException : BusinessException
{
    public DiscordGuildUnavailableException()
        : base(HttpStatusCode.NotFound, "Discord guild is not available.")
    {
    }
}
