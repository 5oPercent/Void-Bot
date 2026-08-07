using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public class ScalarSetting : IConfigurationSetting
{
    public bool Enable { get; }

    public bool ForceHttps { get; }

    public ScalarSetting(IConfiguration configuration)
    {
        Enable = configuration.GetValue<bool>("Scalar:Enable");
        ForceHttps = configuration.GetValue<bool>("Scalar:ForceHttps");
    }
}
