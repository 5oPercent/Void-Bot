using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class DisableTwoFactorCommandValidator
    : FluentMessageValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Two-factor code is required")
            .Matches("^(?:[0-9]{6}|[A-HJ-NP-Za-hj-np-z2-9]{4}-?[A-HJ-NP-Za-hj-np-z2-9]{4}-?[A-HJ-NP-Za-hj-np-z2-9]{4})$")
            .WithMessage("Two-factor code must be a 6-digit TOTP or a valid recovery code");
    }
}
