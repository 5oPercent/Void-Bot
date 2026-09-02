using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class DiscordGuildManagementRequiredException : BusinessException
{
    public DiscordGuildManagementRequiredException()
        : base(HttpStatusCode.Forbidden, "You do not have permission to manage this Discord guild.")
    {
    }
}
