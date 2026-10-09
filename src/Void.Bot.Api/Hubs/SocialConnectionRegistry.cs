using System.Collections.Concurrent;

namespace Void.Bot.Api.Hubs;

public sealed class SocialConnection
{
    public string ConnectionId { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public Guid? EraClientId { get; set; }
    public int TokenVersion { get; set; }
    public DateTimeOffset TokenExpiresAt { get; set; }
    public Action Abort { get; set; } = delegate { };
}

/// <summary>单实例通知连接索引；不用于判断 ERA 是否在线。</summary>
public sealed class SocialConnectionRegistry
{
    private readonly ConcurrentDictionary<string, SocialConnection> _connections;
    private readonly Dictionary<Guid, Dictionary<string, SocialConnection>> _accounts;
    private readonly object _gate;
    private readonly int _maximumConnections;

    public SocialConnectionRegistry(IConfiguration configuration)
    {
        _maximumConnections = configuration.GetValue<int?>("Social:MaximumConnections") ?? 5000;
        if (_maximumConnections < 1)
        {
            throw new InvalidOperationException("Social:MaximumConnections must be positive.");
        }
        _connections = new ConcurrentDictionary<string, SocialConnection>();
        _accounts = new Dictionary<Guid, Dictionary<string, SocialConnection>>();
        _gate = new object();
    }

    public bool TryAdd(SocialConnection connection)
    {
        lock (_gate)
        {
            _accounts.TryGetValue(connection.AccountId, out var account);
            if (_connections.Count >= _maximumConnections || (account != null && account.Count >= 4))
            {
                return false;
            }
            if (!_connections.TryAdd(connection.ConnectionId, connection))
            {
                return false;
            }
            if (account == null)
            {
                account = new Dictionary<string, SocialConnection>();
                _accounts.Add(connection.AccountId, account);
            }
            account.Add(connection.ConnectionId, connection);
            SocialNotificationMetrics.ConnectionOpened();
            return true;
        }
    }

    public void Remove(string connectionId)
    {
        lock (_gate)
        {
            if (!_connections.TryRemove(connectionId, out var connection))
            {
                return;
            }
            if (_accounts.TryGetValue(connection.AccountId, out var account))
            {
                account.Remove(connectionId);
                if (account.Count == 0)
                {
                    _accounts.Remove(connection.AccountId);
                }
            }
            SocialNotificationMetrics.ConnectionClosed();
        }
    }

    public SocialConnection[] Snapshot()
    {
        return _connections.Values.ToArray();
    }

    public SocialConnection[] Snapshot(Guid accountId)
    {
        lock (_gate)
        {
            if (_accounts.TryGetValue(accountId, out var account))
            {
                return account.Values.ToArray();
            }
            return Array.Empty<SocialConnection>();
        }
    }
}
