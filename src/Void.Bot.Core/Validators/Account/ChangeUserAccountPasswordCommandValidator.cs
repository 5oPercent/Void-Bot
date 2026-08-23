using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public class ChangeUserAccountPasswordCommandValidator
    : FluentMessageValidator<ChangeUserAccountPasswordCommand>
{
    public ChangeUserAccountPasswordCommandValidator()
    {
        RuleFor(x => x.OldPassword)
            .NotEmpty().WithMessage("Old password is required");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("New password must contain uppercase letters, lowercase letters, and digits");

        RuleFor(x => x.NewPasswordConfirm)
            .Equal(x => x.NewPassword).WithMessage("New password confirmation does not match");
    }
}
