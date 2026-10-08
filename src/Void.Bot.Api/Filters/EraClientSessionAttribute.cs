using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Void.Bot.Core.Services.ERA;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Api.Filters;

public class EraClientSessionAttribute : TypeFilterAttribute
{
    public EraClientSessionAttribute() : base(typeof(EraClientSessionFilter))
    {
    }
}

public class EraClientSessionFilter : IAsyncActionFilter
{
    private readonly ICurrentUser _currentUser;
    private readonly IEraTeamDataProvider _sessions;

    public EraClientSessionFilter(ICurrentUser currentUser, IEraTeamDataProvider sessions)
    {
        _currentUser = currentUser;
        _sessions = sessions;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var path = context.HttpContext.Request.Path;
        if (path.StartsWithSegments("/api/era/admin"))
        {
            await next().ConfigureAwait(false);
            return;
        }
        if (!_currentUser.Id.HasValue || !Guid.TryParse(context.HttpContext.Request.Headers["X-ERA-Client-Id"].ToString(), out var clientId) || clientId == Guid.Empty)
        {
            context.Result = new OkObjectResult(new EraResponse<bool>
            {
                Code = HttpStatusCode.Unauthorized,
                ErrorCode = "era.client_session_required",
                Message = "缺少有效的游戏客户端会话标识。"
            });
            return;
        }
        var acquire = path.Equals(new Microsoft.AspNetCore.Http.PathString("/api/era/me/initialize")) && HttpMethods.IsPost(context.HttpContext.Request.Method);
        var status = await _sessions.CheckClientSessionAsync(_currentUser.Id.Value, clientId, acquire, context.HttpContext.RequestAborted).ConfigureAwait(false);
        if (status != 1)
        {
            context.Result = new OkObjectResult(new EraResponse<bool>
            {
                Code = status == -1 ? HttpStatusCode.Conflict : HttpStatusCode.Unauthorized,
                ErrorCode = status == -1 ? "era.client_session_in_use" : "era.client_session_expired",
                Message = status == -1 ? "该账号已在另一个客户端游玩，请先退出原客户端。" : "游戏会话已失效，请重新登录。"
            });
            return;
        }
        await next().ConfigureAwait(false);
    }
}
