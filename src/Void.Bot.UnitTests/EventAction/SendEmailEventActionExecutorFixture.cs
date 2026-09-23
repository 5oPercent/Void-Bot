using NSubstitute;
using Shouldly;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Email;
using Void.Bot.Core.Services.EventAction;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Dto.Email;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.UnitTest.EventAction;

public sealed class SendEmailEventActionExecutorFixture
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    private readonly IAccountDataProvider _accountDataProvider = Substitute.For<IAccountDataProvider>();
    private readonly IEmailService _emailService = Substitute.For<IEmailService>();

    [Fact]
    public void DefinitionSupportsUserAccountEvents()
    {
        var definition = CreateExecutor().Definition;

        definition.Type.ShouldBe(SendEmailEventActionExecutor.ActionType);
        definition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountRegistered);
        definition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountPasswordForceReset);
        definition.SupportedEvents.ShouldContain(BuiltInEventNames.AccountThirdPartyForceUnlinked);
        definition.Arguments.ShouldContain(x => x.Name == "Subject" && x.Required);
        definition.Arguments.ShouldContain(x => x.Name == "Content" && x.Required);
    }

    [Fact]
    public async Task ValidationRejectsMissingContentAsync()
    {
        var error = await CreateExecutor().ValidateAsync(
            BuiltInEventNames.AccountPasswordForceReset,
            new Dictionary<string, string[]>
            {
                ["Subject"] = ["Password reset"]
            },
            CancellationToken.None);

        error.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task ValidationRejectsPasswordPlaceholderInSubjectAsync()
    {
        var error = await CreateExecutor().ValidateAsync(
            BuiltInEventNames.AccountPasswordForceReset,
            CreateArguments("Temporary password: {Password}", "Your temporary password is {Password}."),
            CancellationToken.None);

        error.ShouldNotBeNull();
        error.ShouldContain("Subject cannot contain the Password placeholder");
    }

    [Fact]
    public async Task ExecuteSendsToVerifiedEventUserAndReplacesPlaceholdersAsync()
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

        await CreateExecutor().ExecuteAsync(
            new AccountThirdPartyForceUnlinkedEvent(UserId, AppPlatformEnum.Discord),
            CreateArguments(
                "{DisplayName} authorization changed",
                "User {UserId} was unlinked from {Platform}."),
            CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessageDto>(message =>
                message.To == "user@example.com" &&
                message.Subject == "VoidUser#0001 authorization changed" &&
                message.TextContent == $"User {UserId} was unlinked from Discord."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task PasswordForceResetEmailCanIncludeNewPasswordAsync()
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

        await CreateExecutor().ExecuteAsync(
            new AccountPasswordForceResetEvent(UserId, "NewPassword1"),
            CreateArguments(
                "Password reset",
                "Your temporary password is {Password}."),
            CancellationToken.None);

        await _emailService.Received(1).SendAsync(
            Arg.Is<EmailMessageDto>(message =>
                message.To == "user@example.com" &&
                message.TextContent == "Your temporary password is NewPassword1."),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ExecuteRejectsUserWithoutVerifiedEmailAsync()
    {
        _accountDataProvider.GetUserAccountAsync(
                UserId, string.Empty, Arg.Any<CancellationToken>())
            .Returns(new UserAccountDto
            {
                Id = UserId,
                Name = "VoidUser",
                Suffix = "0001",
                Email = "user@example.com"
            });

        await Should.ThrowAsync<InvalidOperationException>(() =>
            CreateExecutor().ExecuteAsync(
                new AccountPasswordForceResetEvent(UserId, "NewPassword1"),
                CreateArguments("Password reset", "Your password was reset."),
                CancellationToken.None));

        await _emailService.DidNotReceive().SendAsync(
            Arg.Any<EmailMessageDto>(),
            Arg.Any<CancellationToken>());
    }

    private SendEmailEventActionExecutor CreateExecutor()
    {
        return new SendEmailEventActionExecutor(_accountDataProvider, _emailService);
    }

    private static Dictionary<string, string[]> CreateArguments(string subject, string content)
    {
        return new Dictionary<string, string[]>
        {
            ["Subject"] = [subject],
            ["Content"] = [content]
        };
    }
}