using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class SendRegistrationEmailCodeCommandValidator
    : FluentMessageValidator<SendRegistrationEmailCodeCommand>
{
    public SendRegistrationEmailCodeCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email is invalid")
            .MaximumLength(320).WithMessage("Email must not exceed 320 characters");
    }
}
