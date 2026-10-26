using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class EraArmorException : BusinessException
{
    public EraArmorException(HttpStatusCode status, string message, string errorCode) : base(status, message, errorCode)
    {
    }
}
