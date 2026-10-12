using System.Net;

namespace Void.Bot.Core.Exceptions;

/// <summary>
/// ERA 玩家预加载失败，由统一异常响应过滤器返回业务错误。
/// </summary>
public sealed class EraPlayerAccessException : BusinessException
{
    public EraPlayerAccessException(HttpStatusCode code, string errorCode, string message)
        : base(code, message, errorCode)
    {
    }
}
