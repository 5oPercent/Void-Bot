using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class EraWorldAccessException : BusinessException
{
    public EraWorldAccessException(HttpStatusCode code, string errorCode, string message) : base(code, message, errorCode) { }
}
