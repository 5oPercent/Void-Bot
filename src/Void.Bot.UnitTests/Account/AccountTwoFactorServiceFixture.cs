using NSubstitute;
using Shouldly;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Security;
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
    private readonly AccountTwoFactorService _service;

    public AccountTwoFactorServiceFixture()
    {
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(Now);
        _service = new AccountTwoFactorService(
            _dataProvider,
            _totpService,
            _secretProtector,
            new CustomCurrentUser(CurrentUserId, "VoidUser#0001"),
            timeProvider,
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
        account.TwoFactorSecret.ShouldBe("protected-secret");
        await _dataProvider.Received(1).UpdateUserAccountAsync(account, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task EnableRequiresAValidCodeAndAdvancesTokenVersionAsync()
    {
        var account = CreateAccount();
        account.TwoFactorSecret = "protected-secret";
        _dataProvider.GetUserAccountAsync(CurrentUserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(account);
        _secretProtector.Unprotect("protected-secret", $"account-two-factor:{CurrentUserId:N}")
            .Returns("BASE32SECRET");
        _totpService.VerifyCode("BASE32SECRET", "123456").Returns(true);

        var response = await _service.EnableAsync(
            new EnableTwoFactorCommand { Code = "123456" }, CancellationToken.None);

        response.Code.ShouldBe(default);
        account.TwoFactorEnabledAt.ShouldBe(Now);
        account.TokenVersion.ShouldBe(1);
        await _dataProvider.Received(1).UpdateUserAccountAsync(account, Arg.Any<CancellationToken>());
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
