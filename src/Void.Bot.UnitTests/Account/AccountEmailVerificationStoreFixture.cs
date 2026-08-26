using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.Account;
using Void.Bot.Core.Settings.System;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum.Account;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Account;

public sealed class AccountEmailVerificationStoreFixture
{
    private readonly ICacheManager _cacheManager = Substitute.For<ICacheManager>();
    private readonly AccountEmailVerificationStore _store;

    public AccountEmailVerificationStoreFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["SecretProtection:EncryptionKey"] = Convert.ToBase64String(new byte[32]),
                ["AccountEmail:CodeTtlMinutes"] = "10",
                ["AccountEmail:ResendIntervalSeconds"] = "60",
                ["AccountEmail:MaxSendsPerHour"] = "5",
                ["AccountEmail:MaxAttempts"] = "5"
            })
            .Build();
        _cacheManager.TryLockAsync(
                Arg.Any<string>(), Arg.Any<TimeSpan>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _store = new AccountEmailVerificationStore(
            _cacheManager,
            new SecretProtector(new SecretProtectionSetting(configuration)),
            new AccountEmailSetting(configuration),
            Substitute.For<ILogger>());
    }

    [Fact]
    public async Task CreateProtectsSixDigitCodeAsync()
    {
        var result = await _store.CreateAsync(
            "user@example.com",
            EmailVerificationPurposeEnum.Registration,
            null,
            CancellationToken.None);

        var created = result.ShouldBeOfType<CreateEmailVerificationResult.Created>();
        created.Code.Length.ShouldBe(6);
        created.Code.All(char.IsDigit).ShouldBeTrue();
        created.Record.ProtectedCode.ShouldNotBe(created.Code);
        created.Record.Email.ShouldBe("user@example.com");
    }

    [Fact]
    public async Task VerifyConsumesMatchingChallengeAsync()
    {
        var created = (await _store.CreateAsync(
                "user@example.com",
                EmailVerificationPurposeEnum.Binding,
                Guid.Parse("11111111-1111-1111-1111-111111111111"),
                CancellationToken.None))
            .ShouldBeOfType<CreateEmailVerificationResult.Created>();
        _cacheManager.Get<AccountEmailVerificationRecord>(
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(created.Record);
        _cacheManager.Take<AccountEmailVerificationRecord>(
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(created.Record);

        var result = await _store.VerifyAndTakeAsync(
            new EmailVerificationDto
            {
                Email = created.Record.Email,
                ChallengeId = created.Record.ChallengeId,
                Code = created.Code
            },
            created.Record.Purpose,
            created.Record.UserId,
            CancellationToken.None);

        result.ShouldBeOfType<VerifyEmailVerificationResult.Verified>();
    }

    [Fact]
    public async Task VerifyRejectsDifferentPurposeAsync()
    {
        var created = (await _store.CreateAsync(
                "user@example.com",
                EmailVerificationPurposeEnum.Registration,
                null,
                CancellationToken.None))
            .ShouldBeOfType<CreateEmailVerificationResult.Created>();
        _cacheManager.Get<AccountEmailVerificationRecord>(
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(created.Record);

        var result = await _store.VerifyAndTakeAsync(
            new EmailVerificationDto
            {
                Email = created.Record.Email,
                ChallengeId = created.Record.ChallengeId,
                Code = created.Code
            },
            EmailVerificationPurposeEnum.PasswordReset,
            null,
            CancellationToken.None);

        result.ShouldBeOfType<VerifyEmailVerificationResult.Invalid>();
    }

    [Fact]
    public async Task UnbindingChallengeIsScopedToCurrentUserAsync()
    {
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var created = (await _store.CreateAsync(
                "user@example.com",
                EmailVerificationPurposeEnum.Unbinding,
                userId,
                CancellationToken.None))
            .ShouldBeOfType<CreateEmailVerificationResult.Created>();
        _cacheManager.Get<AccountEmailVerificationRecord>(
                Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(created.Record);

        var result = await _store.VerifyAndTakeAsync(
            new EmailVerificationDto
            {
                Email = created.Record.Email,
                ChallengeId = created.Record.ChallengeId,
                Code = created.Code
            },
            EmailVerificationPurposeEnum.Unbinding,
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            CancellationToken.None);

        result.ShouldBeOfType<VerifyEmailVerificationResult.Invalid>();
    }
}
