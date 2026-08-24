using Shouldly;
using Void.Bot.Core.Validators.Account;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.UnitTest.Account;

public sealed class AccountEmailValidatorFixture
{
    [Fact]
    public void RegistrationAllowsMissingEmailVerification()
    {
        var result = new RegisterUserAccountCommandValidator().Validate(
            new RegisterUserAccountCommand
            {
                Name = "User",
                Password = "Password1"
            });

        result.IsValid.ShouldBeTrue();
    }

    [Fact]
    public void PasswordResetRequiresValidEmailAndCode()
    {
        var result = new ResetUserAccountPasswordCommandValidator().Validate(
            new ResetUserAccountPasswordCommand
            {
                Email = "invalid",
                ChallengeId = Guid.Empty,
                Code = "123",
                NewPassword = "Password1",
                NewPasswordConfirm = "Password1"
            });

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void EmailUnbindRequiresPassword()
    {
        var result = new UnbindUserEmailCommandValidator().Validate(
            new UnbindUserEmailCommand
            {
                Password = string.Empty
            });

        result.IsValid.ShouldBeFalse();
    }
}
