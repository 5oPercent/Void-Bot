using System.Net;

namespace Void.Bot.Core.Exceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException(HttpStatusCode code, string message)
        : base(message)
    {
        Code = code;
    }

    public HttpStatusCode Code { get; }
}
