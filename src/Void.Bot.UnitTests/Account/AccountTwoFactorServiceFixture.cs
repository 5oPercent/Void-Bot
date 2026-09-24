using Microsoft.Extensions.Configuration;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
using Void.Bot.Core.Settings.Account;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Enum.Account;
using ILogger = Serilog.ILogger;

namespace Void.Bot.UnitTest.Account;

public sealed class AccountTwoFactorServiceFixture
{
    private static readonly Guid CurrentUserId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
    private static readonly DateTimeOffset Now = new(2026, 9, 9, 1, 0, 0, TimeSpan.Zero);
    private readonly IAccountDataProvider _dataProvider = Substitute.For<IAccountDataProvider>();
    private readonly ITotpService _totpService = Substitute.For<ITotpService>();
    private readonly ISecretProtector _secretProtector = Substitute.For<ISecretProtector>();
    private readonly TimeProvider _timeProvider = Substitute.For<TimeProvider>();
    private readonly AccountTwoFactorService _service;

    public AccountTwoFactorServiceFixture()
    {
        _timeProvider.GetUtcNow().Returns(Now);
        _service = new AccountTwoFactorService(
            _dataProvider,
            _totpService,
            _secretProtector,
            new TwoFactorSetting(new ConfigurationBuilder().Build()),
            new CustomCurrentUser(CurrentUserId, "VoidUser#0001"),
            _timeProvider,
            Substitute.For<ILogger>());
    }

    [Fact]
    public async Task BeginSetupUsesCurrentUserAndStoresProtectedSecretAsync()
    {
        var account = CreateAccount();
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _totpService.GenerateSecret().Returns("BASE32SECRET");
        _secretProtector.Protect("BASE32SECRET", $"account-two-factor:{CurrentUserId:N}")
            .Returns("protected-secret");
        _totpService.CreateOtpAuthUri("Void Bot", account.DisplayName, "BASE32SECRET")
            .Returns("otpauth://setup");

        var response = await _service.BeginSetupAsync(
            new BeginTwoFactorSetupCommand { Password = "Password1" }, CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.ManualEntryKey.ShouldBe("BASE32SECRET");
        response.Data.ExpiresAt.ShouldBe(Now.AddMinutes(10));
        account.TwoFactorSecret.ShouldBe("protected-secret");
        account.TwoFactorSetupAt.ShouldBe(Now);
        await _dataProvider.Received(1).UpdateUserAccountAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BeginSetupUsesConfiguredTitleAsync()
    {
        var account = CreateAccount();
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _totpService.GenerateSecret().Returns("BASE32SECRET");
        _secretProtector.Protect("BASE32SECRET", $"account-two-factor:{CurrentUserId:N}")
            .Returns("protected-secret");

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["TwoFactor:Title"] = "Void Bot Staging"
            })
            .Build();
        var service = new AccountTwoFactorService(
            _dataProvider,
            _totpService,
            _secretProtector,
            new TwoFactorSetting(configuration),
            new CustomCurrentUser(CurrentUserId, "VoidUser#0001"),
            TimeProvider.System,
            Substitute.For<ILogger>());

        await service.BeginSetupAsync(
            new BeginTwoFactorSetupCommand { Password = "Password1" }, CancellationToken.None);

        _totpService.Received(1).CreateOtpAuthUri(
            "Void Bot Staging", account.DisplayName, "BASE32SECRET");
    }

    [Fact]
    public async Task StatusIncludesRemainingRecoveryCodeCountAsync()
    {
        var account = CreateAccount();
        account.TwoFactorEnabledAt = Now;
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _dataProvider.GetTwoFactorRecoveryCodeCountAsync(
                CurrentUserId, Arg.Any<CancellationToken>())
            .Returns(3);

        var response = await _service.GetStatusAsync(CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.RecoveryCodesRemaining.ShouldBe(3);
    }

    [Fact]
    public async Task EnableRequiresAValidCodeAndAdvancesTokenVersionAsync()
    {
        var account = CreateAccount();
        account.TwoFactorSecret = "protected-secret";
        account.TwoFactorSetupAt = Now;
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _secretProtector.Unprotect("protected-secret", $"account-two-factor:{CurrentUserId:N}")
            .Returns("BASE32SECRET");
        _totpService.VerifyCode("BASE32SECRET", "123456").Returns(true);

        var response = await _service.EnableAsync(
            new EnableTwoFactorCommand { Code = "123456" }, CancellationToken.None);

        response.Code.ShouldBe(default);
        account.TwoFactorEnabledAt.ShouldBe(Now);
        account.TwoFactorSetupAt.ShouldBeNull();
        account.TokenVersion.ShouldBe(1);
        response.Data.ShouldNotBeNull();
        response.Data.RecoveryCodes.Count.ShouldBe(8);
        await _dataProvider.Received(1).UpdateUserAccountAsync(account, Arg.Any<CancellationToken>());
        await _dataProvider.Received(1).ReplaceTwoFactorRecoveryCodesAsync(
            account.Id,
            Arg.Is<IReadOnlyCollection<string>>(hashes => hashes.Count == 8 && hashes.All(x => x.Length == 64)),
            Now,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnableRejectsAndClearsExpiredSetupAsync()
    {
        var account = CreateAccount();
        account.TwoFactorSecret = "protected-secret";
        account.TwoFactorSetupAt = Now.AddMinutes(-11);
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);

        var response = await _service.EnableAsync(
            new EnableTwoFactorCommand { Code = "123456" }, CancellationToken.None);

        response.Message.ShouldBe("two_factor_setup_expired");
        account.TwoFactorSecret.ShouldBeNull();
        account.TwoFactorSetupAt.ShouldBeNull();
        await _dataProvider.Received(1).UpdateUserAccountAsync(account, Arg.Any<CancellationToken>());
        _totpService.DidNotReceive().VerifyCode(Arg.Any<string>(), Arg.Any<string>());
    }

    [Fact]
    public async Task LoginVerificationUsesAuthenticatedAccountIdInsteadOfCurrentUserAsync()
    {
        var loginAccountId = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        var account = CreateAccount(loginAccountId);
        account.TwoFactorSecret = "protected-secret";
        account.TwoFactorEnabledAt = Now;
        _dataProvider.GetUserAccountAsync(loginAccountId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _secretProtector.Unprotect("protected-secret", $"account-two-factor:{loginAccountId:N}")
            .Returns("BASE32SECRET");
        _totpService.VerifyCode("BASE32SECRET", "123456").Returns(true);

        var result = await _service.VerifyLoginAsync(loginAccountId, "123456", CancellationToken.None);

        result.ShouldBe(TwoFactorLoginVerificationResult.Valid);
        await _dataProvider.DidNotReceive().GetUserAccountAsync(
            CurrentUserId, string.Empty, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task LoginVerificationConsumesRecoveryCodeAsync()
    {
        var account = CreateAccount();
        account.TwoFactorSecret = "protected-secret";
        account.TwoFactorEnabledAt = Now;
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _secretProtector.Unprotect("protected-secret", $"account-two-factor:{CurrentUserId:N}")
            .Returns("BASE32SECRET");
        _totpService.VerifyCode("BASE32SECRET", "ABCD-EFGH-JK23").Returns(false);
        _dataProvider.TryConsumeTwoFactorRecoveryCodeAsync(
                CurrentUserId, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);

        var result = await _service.VerifyLoginAsync(
            CurrentUserId, "abcd-efgh-jk23", CancellationToken.None);

        result.ShouldBe(TwoFactorLoginVerificationResult.Valid);
        await _dataProvider.Received(1).TryConsumeTwoFactorRecoveryCodeAsync(
            CurrentUserId,
            Arg.Is<string>(hash => hash.Length == 64),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RegenerateRecoveryCodesReplacesExistingCodesAsync()
    {
        var account = CreateAccount();
        account.TwoFactorSecret = "protected-secret";
        account.TwoFactorEnabledAt = Now;
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _secretProtector.Unprotect("protected-secret", $"account-two-factor:{CurrentUserId:N}")
            .Returns("BASE32SECRET");
        _totpService.VerifyCode("BASE32SECRET", "123456").Returns(true);

        var response = await _service.RegenerateRecoveryCodesAsync(
            new RegenerateTwoFactorRecoveryCodesCommand
            {
                Password = "Password1",
                Code = "123456"
            }, CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.RecoveryCodes.Count.ShouldBe(8);
        await _dataProvider.Received(1).ReplaceTwoFactorRecoveryCodesAsync(
            account.Id,
            Arg.Is<IReadOnlyCollection<string>>(hashes => hashes.Count == 8),
            Now,
            Arg.Any<CancellationToken>());
    }

    private static UserAccountDto CreateAccount(Guid? id = null)
    {
        return new UserAccountDto
        {
            Id = id ?? CurrentUserId,
            Name = "VoidUser",
            Suffix = "0001",
            Password = "Password1".SHAEncrypt()
        };
    }
}
