using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.System;

namespace Void.Bot.UnitTest.Security;

public class SecretProtectorFixture
{
    [Fact]
    public void ProtectAndUnprotectRoundTripsSecret()
    {
        var protector = CreateProtector();

        var encrypted = protector.Protect("secret-value", "test-purpose");

        encrypted.ShouldNotBe("secret-value");
        protector.Unprotect(encrypted, "test-purpose").ShouldBe("secret-value");
    }

    [Fact]
    public void ProtectUsesRandomNonce()
    {
        var protector = CreateProtector();

        var first = protector.Protect("secret-value", "test-purpose");
        var second = protector.Protect("secret-value", "test-purpose");

        first.ShouldNotBe(second);
    }

    [Fact]
    public void UnprotectRejectsDifferentPurpose()
    {
        var protector = CreateProtector();
        var encrypted = protector.Protect("secret-value", "first-purpose");

        Should.Throw<CryptographicException>(() =>
            protector.Unprotect(encrypted, "second-purpose"));
    }

    private static SecretProtector CreateProtector()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretProtection:EncryptionKey"] = Convert.ToBase64String(new byte[32])
            })
            .Build();
        return new SecretProtector(new SecretProtectionSetting(configuration));
    }
}
