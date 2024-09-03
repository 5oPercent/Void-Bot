using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class ConnectionStringSetting : IConfigurationSetting<string>
{
    public string Value { get; set; }
    
    public ConnectionStringSetting(IConfiguration configuration)
    {
        Value = configuration["ConnectionStrings"] ?? string.Empty;
    }
}