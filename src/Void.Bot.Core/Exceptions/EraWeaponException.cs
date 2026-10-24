using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class EraWeaponException : BusinessException
{
    public EraWeaponException(HttpStatusCode status, string message, string errorCode) : base(status, message, errorCode)
    {
    }
}
