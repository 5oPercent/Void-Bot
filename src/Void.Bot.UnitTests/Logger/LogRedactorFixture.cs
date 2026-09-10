using System.Text.Json;
using Shouldly;
using Void.Bot.Core.Middlewares.Logger;
using Void.Bot.Message.Commands.Account;
using Void.Bot.Message.Dto.Account;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.UnitTest.Logger;

public sealed class LogRedactorFixture
{
    [Fact]
    public void PasswordFieldsAreRedactedRegardlessOfPrefixOrSuffix()
    {
        var command = new ForceResetUserAccountPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = "NewPassword1",
            NewPasswordConfirm = "NewPassword1"
        };

        var redacted = LogRedactor.Redact(command)
            .ShouldBeOfType<Dictionary<string, object?>>();

        redacted["NewPassword"].ShouldBe("[REDACTED]");
        redacted["NewPasswordConfirm"].ShouldBe("[REDACTED]");
        LogRedactor.ContainsSensitiveValues(command).ShouldBeTrue();
    }

    [Fact]
    public void PasswordForceResetEventDoesNotExposePasswordToLogs()
    {
        var builtInEvent = new AccountPasswordForceResetEvent(
            Guid.NewGuid(),
            "NewPassword1");

        var redacted = LogRedactor.Redact(builtInEvent)
            .ShouldBeOfType<Dictionary<string, object?>>();

        redacted["Password"].ShouldBe("[REDACTED]");
        LogRedactor.ContainsSensitiveValues(builtInEvent).ShouldBeTrue();
        builtInEvent.ToString().ShouldNotContain("NewPassword1");
        JsonSerializer.Serialize(builtInEvent).ShouldNotContain("NewPassword1");
    }

    [Fact]
    public void TwoFactorRecoveryCodesAreRedactedFromResponseLogs()
    {
        var response = new EnableTwoFactorResponse
        {
            Data = new TwoFactorRecoveryCodesDto
            {
                RecoveryCodes = ["ABCD-EFGH-JK23"]
            }
        };

        var redacted = LogRedactor.Redact(response)
            .ShouldBeOfType<Dictionary<string, object?>>();
        var data = redacted["Data"]
            .ShouldBeOfType<Dictionary<string, object?>>();

        data["RecoveryCodes"].ShouldBe("[REDACTED]");
        LogRedactor.ContainsSensitiveValues(response).ShouldBeTrue();
    }

    [Fact]
    public void TwoFactorVerificationCodeIsRedactedFromRequestLogs()
    {
        var command = new RegenerateTwoFactorRecoveryCodesCommand
        {
            Password = "Password1",
            Code = "ABCD-EFGH-JK23"
        };

        var redacted = LogRedactor.Redact(command)
            .ShouldBeOfType<Dictionary<string, object?>>();

        redacted["Password"].ShouldBe("[REDACTED]");
        redacted["Code"].ShouldBe("[REDACTED]");
        LogRedactor.ContainsSensitiveValues(command).ShouldBeTrue();
    }
}
