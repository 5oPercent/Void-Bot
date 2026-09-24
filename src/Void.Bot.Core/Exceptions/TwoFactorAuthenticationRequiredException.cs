using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class TwoFactorAuthenticationRequiredException : BusinessException
{
    public TwoFactorAuthenticationRequiredException()
        : base(HttpStatusCode.Unauthorized, "two_factor_authentication_required")
    {
    }
}
