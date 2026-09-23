using Microsoft.Extensions.Configuration;
using Void.Bot.Message.Enum;

namespace Void.Bot.Core.Settings.System;

public class HangfireSetting(IConfiguration configuration) : IConfigurationSetting
{
    public bool EnableDashboard { get; } = configuration.GetValue<bool>("Hangfire:EnableDashboard");
    public CacheTypeEnum UseStorage { get; } = configuration.GetValue<CacheTypeEnum>("Cache:UseStorage");
    public string DashboardPath { get; } = configuration["Hangfire:DashboardPath"] ?? "/hangfire";
    public string DashboardUser { get; } = configuration["Hangfire:DashboardUser"] ?? "admin";
    public string DashboardPassword { get; } = configuration["Hangfire:DashboardPassword"] ?? "hangfire";
}