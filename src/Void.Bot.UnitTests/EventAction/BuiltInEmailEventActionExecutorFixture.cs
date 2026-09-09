using System.Linq.Expressions;
using NSubstitute;
using Shouldly;
using Void.Bot.Core.Resources.i18n.Email;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Email;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.UnitTest.EventAction;

public sealed class BuiltInEmailEventActionExecutorFixture
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IAccountDataProvider _accountDataProvider = Substitute.For<IAccountDataProvider>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();
    private readonly ILocalizationService _localizationService = Substitute.For<ILocalizationService>();

    public BuiltInEmailEventActionExecutorFixture()
    {
        _localizationService.GetValue<BuiltInEmailContextTemplate>(
                Arg.Any<Expression<Func<BuiltInEmailContextTemplate, object>>>(),
                Arg.Any<LanguageEnum>(),
                Arg.Any<object[]>())
            .Returns(callInfo =>
            {
                var expression = callInfo.ArgAt<Expression<Func<BuiltInEmailContextTemplate, object>>>(0);
                var value = expression.Compile()(new BuiltInEmailContextTemplate()).ToString() ?? string.Empty;
                var arguments = callInfo.ArgAt<object[]>(2);
                return arguments.Length == 0 ? value : string.Format(value, arguments);
            });
    }

    [Fact]
    public void DefinitionsAreArgumentFreeAndEventSpecific()
    {
        var registeredDefinition = CreateRegisteredExecutor().Definition;
        var passwordDefinition = CreatePasswordExecutor().Definition;
        var unlinkDefinition = CreateUnlinkExecutor().Definition;

        registeredDefinition.Type.ShouldBe(SendAccountRegisteredEmailEventActionExecutor.ActionType);
        registeredDefinition.SupportedEvents.ShouldBe([BuiltInEventNames.AccountRegistered]);
        registeredDefinition.Arguments.ShouldBeEmpty();
        passwordDefinition.Type.ShouldBe(SendPasswordForceResetEmailEventActionExecutor.ActionType);
        passwordDefinition.SupportedEvents.ShouldBe([BuiltInEventNames.AccountPasswordForceReset]);
        passwordDefinition.Arguments.ShouldBeEmpty();
        unlinkDefinition.Type.ShouldBe(SendThirdPartyForceUnlinkedEmailEventActionExecutor.ActionType);
        unlinkDefinition.SupportedEvents.ShouldBe([BuiltInEventNames.AccountThirdPartyForceUnlinked]);
        unlinkDefinition.Arguments.ShouldBeEmpty();
    }

    [Fact]
    public async Task ValidationRejectsArgumentsAsync()
    {
        var error = await CreatePasswordExecutor().ValidateAsync(
            BuiltInEventNames.AccountPasswordForceReset,
            new Dictionary<string, string[]> { ["Content"] = ["Custom content"] },
            CancellationToken.None);

        error.ShouldContain("does not accept arguments");
    }

    [Fact]
    public async Task PasswordResetActionSendsLocalizedBuiltInContentAsync()
    {
        ConfigureVerifiedAccount();

        await CreatePasswordExecutor().ExecuteAsync(
            new AccountPasswordForceResetEvent(UserId, "NewPassword1"),
            new Dictionary<string, string[]>(),
            CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessageDto>(message =>
                message.To == "user@example.com" &&
                message.Subject == "Void Bot 密码已被重置" &&
                message.TextContent.Contains("VoidUser#0001") &&
                message.TextContent.Contains("NewPassword1")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task AccountRegisteredActionSendsLocalizedBuiltInContentAsync()
    {
        ConfigureVerifiedAccount();

        await CreateRegisteredExecutor().ExecuteAsync(
            new AccountRegisteredEvent(UserId),
            new Dictionary<string, string[]>(),
            CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessageDto>(message =>
                message.To == "user@example.com" &&
                message.Subject == "欢迎使用 Void Bot" &&
                message.TextContent.Contains("VoidUser#0001") &&
                message.TextContent.Contains("账号已注册成功")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ThirdPartyUnlinkActionSendsLocalizedBuiltInContentAsync()
    {
        ConfigureVerifiedAccount();

        await CreateUnlinkExecutor().ExecuteAsync(
            new AccountThirdPartyForceUnlinkedEvent(UserId, AppPlatformEnum.Discord),
            new Dictionary<string, string[]>(),
            CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessageDto>(message =>
                message.To == "user@example.com" &&
                message.Subject == "Void Bot 第三方授权已被解绑" &&
                message.TextContent.Contains("VoidUser#0001") &&
                message.TextContent.Contains("Discord")),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task BuiltInActionRejectsUserWithoutVerifiedEmailAsync()
    {
        _accountDataProvider.GetUserAccountAsync(
                UserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto
            {
                Id = UserId,
                Email = "user@example.com"
            });

        await Should.ThrowAsync<InvalidOperationException>(() =>
            CreatePasswordExecutor().ExecuteAsync(
                new AccountPasswordForceResetEvent(UserId, "NewPassword1"),
                new Dictionary<string, string[]>(),
                CancellationToken.None));

        await _emailService.DidNotReceive().SendAsync(
            Arg.Any<EmailMessageDto>(),
            Arg.Any<CancellationToken>());
    }

    private void ConfigureVerifiedAccount()
    {
        _accountDataProvider.GetUserAccountAsync(
                UserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto
            {
                Id = UserId,
                Name = "VoidUser",
                Suffix = "0001",
                Email = "user@example.com",
                EmailVerifiedAt = DateTimeOffset.UtcNow
            });
    }

    private SendPasswordForceResetEmailEventActionExecutor CreatePasswordExecutor()
    {
        return new SendPasswordForceResetEmailEventActionExecutor(
            _accountDataProvider, _emailService, _localizationService);
    }

    private SendAccountRegisteredEmailEventActionExecutor CreateRegisteredExecutor()
    {
        return new SendAccountRegisteredEmailEventActionExecutor(
            _accountDataProvider, _emailService, _localizationService);
    }

    private SendThirdPartyForceUnlinkedEmailEventActionExecutor CreateUnlinkExecutor()
    {
        return new SendThirdPartyForceUnlinkedEmailEventActionExecutor(
            _accountDataProvider, _emailService, _localizationService);
    }
}
