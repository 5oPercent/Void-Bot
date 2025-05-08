using Microsoft.Extensions.Configuration;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Settings.System;

public class HangfireSetting : IConfigurationSetting
{
    public bool EnableDashboard { get; }
    
    public string DashboardPath { get; }
    
    public string DashboardUser { get; }
    
    public string DashboardPassword { get; }
    
    public CacheTypeEnum UseStorage { get; }

    public HangfireSetting(IConfiguration configuration)
    {
        EnableDashboard = configuration.GetValue<bool>("Hangfire:EnableDashboard");
        
        DashboardPath = configuration.GetValue<string>("Hangfire:DashboardPath") ?? "/hangfire";
        
        DashboardUser = configuration.GetValue<string>("Hangfire:DashboardUser") ?? "admin";
        
        DashboardPassword = configuration.GetValue<string>("Hangfire:DashboardPassword") ?? "hangfire";
        
        UseStorage = configuration.GetValue<CacheTypeEnum>("Cache:UseStorage");
    }
}