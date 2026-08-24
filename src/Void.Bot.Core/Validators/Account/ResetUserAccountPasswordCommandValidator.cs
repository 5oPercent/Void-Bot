using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class ResetUserAccountPasswordCommandValidator
    : FluentMessageValidator<ResetUserAccountPasswordCommand>
{
    public ResetUserAccountPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters");

        RuleFor(x => x.ChallengeId)
            .NotEmpty().WithMessage("Email verification challenge is required");

        RuleFor(x => x.Code)
            .Matches(@"^\d{6}$").WithMessage("Email verification code must contain 6 digits");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("New password must contain uppercase letters, lowercase letters, and digits");

        RuleFor(x => x.NewPasswordConfirm)
            .Equal(x => x.NewPassword).WithMessage("New password confirmation does not match");
    }
}
