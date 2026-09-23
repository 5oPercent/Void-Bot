using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class ConnectionStringSetting(IConfiguration configuration) : IConfigurationSetting
{
    public string Mysql { get; } = configuration["ConnectionStrings:Mysql"] ?? string.Empty;
    public string Redis { get; } = configuration["ConnectionStrings:Redis"] ?? string.Empty;
}