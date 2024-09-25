using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class ConnectionStringSetting : IConfigurationSetting
{
    public string Mysql { get; set; }
    
    public ConnectionStringSetting(IConfiguration configuration)
    {
        Mysql = configuration["ConnectionStrings:Mysql"] ?? string.Empty;
    }
}