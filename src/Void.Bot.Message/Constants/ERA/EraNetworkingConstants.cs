namespace Void.Bot.Message.Constants.ERA;

public static class EraNetworkingConstants
{
    // 同一 hash tag 保证队伍、队伍码及玩家索引原子脚本位于同一 Redis Cluster 槽。
    public const string RedisTeamIndex = "era:teams:{era-teams}:v2:index";
    public const string RedisTeamPrefix = "era:teams:{era-teams}:v2:team:";
    public const string RedisPlayerPrefix = "era:teams:{era-teams}:v2:player:";
    public const string RedisTeamCodePrefix = "era:teams:{era-teams}:v2:code:";
    public const int TeamCodeLength = 8;
    public const string TeamCodeAlphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
    public const string TeamCodePattern = "^[ABCDEFGHJKLMNPQRSTUVWXYZ23456789abcdefghjklmnpqrstuvwxyz]{8}$";
    public const string TeamRateLimitPolicy = "EraTeams";
    public const string RegistrationPrefix = "ERA-PUNCH/2 ";
    public const int MaxDatagramBytes = 2048;
    public const int MaxSourcesPerWindow = 256;
    public const int PacketsPerSourcePerSecond = 30;
    public const int HttpRequestsPerSecond = 120;
}
