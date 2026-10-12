using System.Net;

namespace Void.Bot.Core.Exceptions;

public abstract class BusinessException : Exception
{
    protected BusinessException(HttpStatusCode code, string message, string? errorCode = null)
        : base(message)
    {
        Code = code;
        ErrorCode = errorCode;
    }

    public HttpStatusCode Code { get; }

    public string? ErrorCode { get; }
}
