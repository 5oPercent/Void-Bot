using Microsoft.Extensions.Configuration;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Settings.System;

public class HangfireSetting : IConfigurationSetting
{
    public bool EnableDashboard { get; }
    
    public CacheTypeEnum UseStorage { get; }

    public HangfireSetting(IConfiguration configuration)
    {
        EnableDashboard = configuration.GetValue<bool>("Hangfire:EnableDashboard");
        
        UseStorage = configuration.GetValue<CacheTypeEnum>("Cache:UseStorage");
    }
}