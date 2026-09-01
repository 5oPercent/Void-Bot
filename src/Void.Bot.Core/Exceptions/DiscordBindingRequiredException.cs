using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class DiscordBindingRequiredException : BusinessException
{
    public DiscordBindingRequiredException()
        : base(HttpStatusCode.BadRequest, "A Discord account must be bound first.")
    {
    }
}
