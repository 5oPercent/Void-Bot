using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Services.ERA.Team;

namespace Void.Bot.Api.Hubs;

public abstract class SocialHubBase : Hub
{
    private readonly SocialConnectionRegistry _connections;
    private readonly VoidBotDbContext _database;
    private readonly IEraTeamDataProvider _sessions;
    protected abstract bool RequiresEraSession { get; }

    protected SocialHubBase(SocialConnectionRegistry connections,
        VoidBotDbContext database, IEraTeamDataProvider sessions)
    {
        _connections = connections;
        _database = database;
        _sessions = sessions;
    }

    public override async Task OnConnectedAsync()
    {
        var httpContext = Context.GetHttpContext();
        if (httpContext == null ||
            !Guid.TryParse(Context.User?.FindFirstValue(ClaimTypes.NameIdentifier), out var accountId) ||
            !int.TryParse(Context.User?.FindFirstValue("token_version"), out var tokenVersion))
        {
            Context.Abort();
            throw new HubException("social.unauthorized");
        }

        var authentication = await httpContext.AuthenticateAsync("Bearer").ConfigureAwait(false);
        var account = await _database.Set<UserAccount>().AsNoTracking()
            .SingleOrDefaultAsync(value => value.Id == accountId, Context.ConnectionAborted)
            .ConfigureAwait(false);
        if (account == null || account.TokenVersion != tokenVersion ||
            authentication.Properties?.ExpiresUtc == null)
        {
            Context.Abort();
            throw new HubException("social.unauthorized");
        }

        Guid? clientId = null;
        if (RequiresEraSession)
        {
            var value = httpContext.Request.Headers["X-ERA-Client-Id"].ToString();
            if (string.IsNullOrEmpty(value))
            {
                value = httpContext.Request.Query["client_id"].ToString();
            }
            if (!Guid.TryParse(value, out var parsed) || parsed == Guid.Empty ||
                !await _sessions.IsClientSessionActiveAsync(accountId, parsed, Context.ConnectionAborted)
                    .ConfigureAwait(false))
            {
                Context.Abort();
                throw new HubException("era.client_session_expired");
            }
            clientId = parsed;
        }

        var registered = _connections.TryAdd(new SocialConnection
        {
            ConnectionId = Context.ConnectionId,
            AccountId = accountId,
            EraClientId = clientId,
            TokenVersion = tokenVersion,
            TokenExpiresAt = authentication.Properties!.ExpiresUtc!.Value,
            Abort = Context.Abort
        });
        if (!registered)
        {
            Context.Abort();
            throw new HubException("social.connection_limit");
        }
        await base.OnConnectedAsync().ConfigureAwait(false);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _connections.Remove(Context.ConnectionId);
        await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
    }
}
