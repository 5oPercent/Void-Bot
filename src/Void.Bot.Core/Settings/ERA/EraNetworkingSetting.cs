using System.Net;
using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.ERA;

public class EraNetworkingSetting : IConfigurationSetting
{
    public bool Enabled { get; }
    public IPAddress BindAddress { get; }
    /// <summary>公布给客户端的 UDP 主机名：支持 DNS 域名或 IPv4，不包含协议、路径或端口。</summary>
    public string PublicHost { get; }
    public int UdpPort { get; }
    public int MaxMembers { get; }
    public int TeamLifetimeSeconds { get; }
    public int MemberLifetimeSeconds { get; }
    public int TicketLifetimeSeconds { get; }
    public int EndpointLifetimeSeconds { get; }
    public int PacketsPerSecond { get; }
    public int MigrationTimeoutSeconds { get; }

    public EraNetworkingSetting(IConfiguration configuration)
    {
        Enabled = configuration.GetValue<bool>("Era:Networking:Enabled");
        BindAddress = IPAddress.Parse(configuration["Era:Networking:BindAddress"] ?? "0.0.0.0");
        PublicHost = (configuration["Era:Networking:PublicHost"] ?? string.Empty).Trim();
        UdpPort = configuration.GetValue<int?>("Era:Networking:UdpPort") ?? 3398;
        MaxMembers = configuration.GetValue<int?>("Era:Networking:MaxMembers") ?? 4;
        TeamLifetimeSeconds = configuration.GetValue<int?>("Era:Networking:TeamLifetimeSeconds") ?? 120;
        MemberLifetimeSeconds = configuration.GetValue<int?>("Era:Networking:MemberLifetimeSeconds") ?? 30;
        TicketLifetimeSeconds = configuration.GetValue<int?>("Era:Networking:TicketLifetimeSeconds") ?? 60;
        EndpointLifetimeSeconds = configuration.GetValue<int?>("Era:Networking:EndpointLifetimeSeconds") ?? 15;
        PacketsPerSecond = configuration.GetValue<int?>("Era:Networking:PacketsPerSecond") ?? 1000;
        MigrationTimeoutSeconds = configuration.GetValue<int?>("Era:Networking:MigrationTimeoutSeconds") ?? 15;
        if (MigrationTimeoutSeconds < 5 || MigrationTimeoutSeconds > 120 || UdpPort < 1 || UdpPort > 65535 ||
            MaxMembers < 2 || MaxMembers > 16 || MemberLifetimeSeconds < 15 ||
            MemberLifetimeSeconds > 3600 || TeamLifetimeSeconds < MemberLifetimeSeconds ||
            TeamLifetimeSeconds > 86400 || TicketLifetimeSeconds < 15 ||
            TicketLifetimeSeconds > 300 || EndpointLifetimeSeconds < 1 ||
            EndpointLifetimeSeconds > TicketLifetimeSeconds || PacketsPerSecond < 1 || PacketsPerSecond > 100000 ||
            BindAddress.AddressFamily != global::System.Net.Sockets.AddressFamily.InterNetwork)
        {
            throw new InvalidOperationException("Invalid Era networking configuration.");
        }

        if (Enabled)
        {
            var hostType = Uri.CheckHostName(PublicHost);
            var validHost = hostType == UriHostNameType.Dns;
            if (hostType == UriHostNameType.IPv4)
            {
                validHost = IPAddress.TryParse(PublicHost, out var publicAddress) &&
                            publicAddress.AddressFamily == global::System.Net.Sockets.AddressFamily.InterNetwork;
            }
            if (!validHost)
            {
                throw new InvalidOperationException("Era networking requires an advertised DNS hostname or IPv4 address without a scheme, path or port.");
            }
            // 原样公布域名，启动时不固定解析 IP，由客户端在连接时按当前 DNS 记录解析。
        }
    }
}
