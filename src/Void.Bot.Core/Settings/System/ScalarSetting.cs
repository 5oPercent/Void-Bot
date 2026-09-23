using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class ScalarSetting(IConfiguration configuration) : IConfigurationSetting
{
    public bool Enable { get; } = configuration.GetValue<bool>("Scalar:Enable");
    public bool ForceHttps { get; } = configuration.GetValue<bool>("Scalar:ForceHttps");
}