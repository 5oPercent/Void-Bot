using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public sealed class SecretProtectionSetting : IConfigurationSetting
{
    public string EncryptionKey { get; }

    public SecretProtectionSetting(IConfiguration configuration)
    {
        EncryptionKey = configuration["SecretProtection:EncryptionKey"] ?? string.Empty;
    }
}
