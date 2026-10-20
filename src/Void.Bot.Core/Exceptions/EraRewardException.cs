using System.Net;

namespace Void.Bot.Core.Exceptions;

public sealed class EraRewardException : BusinessException
{
    public EraRewardException(HttpStatusCode code, string errorCode, string message) : base(code, message, errorCode)
    {
    }
}
