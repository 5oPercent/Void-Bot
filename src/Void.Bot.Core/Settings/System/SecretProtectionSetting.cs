using Microsoft.Extensions.Configuration;

namespace Void.Bot.Core.Settings.System;

public sealed class SecretProtectionSetting(IConfiguration configuration) : IConfigurationSetting
{
    public string EncryptionKey { get; } = configuration["SecretProtection:EncryptionKey"] ?? string.Empty;
}