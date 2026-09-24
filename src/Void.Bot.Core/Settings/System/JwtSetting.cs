using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class JwtSetting(IConfiguration configuration) : IConfigurationSetting
{
    public string Secret { get; } = configuration["Jwt:Secret"] ?? string.Empty;
    public string Issuer { get; } = configuration["Jwt:Issuer"] ?? string.Empty;
    public string Audience { get; } = configuration["Jwt:Audience"] ?? string.Empty;
    public int ExpireMinutes { get; } = configuration.GetValue<int>("Jwt:ExpireMinutes");
    public int RefreshExpireDays { get; } = configuration.GetValue<int?>("Jwt:RefreshExpireDays") ?? 7;
}