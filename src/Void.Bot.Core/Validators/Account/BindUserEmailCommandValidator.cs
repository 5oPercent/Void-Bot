using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class BindUserEmailCommandValidator : FluentMessageValidator<BindUserEmailCommand>
{
    public BindUserEmailCommandValidator()
    {
        RuleFor(x => x.EmailVerification)
            .NotNull().WithMessage("Email verification is required")
            .SetValidator(new EmailVerificationDtoValidator());
    }
}
