using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Email;

namespace Void.Bot.Core.Validators.Email;

public sealed class SendTestEmailCommandValidator : FluentMessageValidator<SendTestEmailCommand>
{
    public SendTestEmailCommandValidator()
    {
        RuleFor(x => x.To)
            .NotEmpty().WithMessage("Recipient email is required")
            .EmailAddress().WithMessage("Recipient email is invalid")
            .MaximumLength(320).WithMessage("Recipient email is too long");

        RuleFor(x => x.Subject)
            .NotEmpty().WithMessage("Email subject is required")
            .MaximumLength(200).WithMessage("Email subject is too long");

        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Email content is required")
            .MaximumLength(10000).WithMessage("Email content is too long");
    }
}
