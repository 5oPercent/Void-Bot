using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class UnbindUserEmailCommandValidator : FluentMessageValidator<UnbindUserEmailCommand>
{
    public UnbindUserEmailCommandValidator()
    {
        RuleFor(x => x.ChallengeId)
            .NotEmpty().WithMessage("Email verification challenge is required");

        RuleFor(x => x.Code)
            .Matches(@"^\d{6}$").WithMessage("Email verification code must contain 6 digits");
    }
}
