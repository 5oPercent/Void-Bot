using System.Net;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Services.ERA;

public interface IEraClientSessionService : IScope
{
    Task<HeartbeatEraClientSessionResponse> HeartbeatAsync(HeartbeatEraClientSessionCommand command, CancellationToken cancellationToken);
    Task<CloseEraClientSessionResponse> CloseAsync(CloseEraClientSessionCommand command, CancellationToken cancellationToken);
}

public class EraClientSessionService : IEraClientSessionService
{
    private readonly IEraTeamDataProvider _sessions;
    private readonly ICurrentUser _currentUser;

    public EraClientSessionService(IEraTeamDataProvider sessions, ICurrentUser currentUser)
    {
        _sessions = sessions;
        _currentUser = currentUser;
    }

    public async Task<HeartbeatEraClientSessionResponse> HeartbeatAsync(HeartbeatEraClientSessionCommand command, CancellationToken cancellationToken)
    {
        var status = _currentUser.Id.HasValue
            ? await _sessions.CheckClientSessionAsync(_currentUser.Id.Value, command.ClientId, false, cancellationToken).ConfigureAwait(false)
            : 0;
        return new HeartbeatEraClientSessionResponse
        {
            Code = status == 1 ? HttpStatusCode.OK : HttpStatusCode.Unauthorized,
            ErrorCode = status == 1 ? null : "era.client_session_expired",
            Message = status == 1 ? string.Empty : "游戏会话已失效，请重新登录。",
            Data = status == 1
        };
    }

    public async Task<CloseEraClientSessionResponse> CloseAsync(CloseEraClientSessionCommand command, CancellationToken cancellationToken)
    {
        if (_currentUser.Id.HasValue)
        {
            await _sessions.ReleaseClientSessionAsync(_currentUser.Id.Value, command.ClientId, cancellationToken).ConfigureAwait(false);
        }
        return new CloseEraClientSessionResponse
        {
            Code = HttpStatusCode.OK,
            Data = true
        };
    }
}
