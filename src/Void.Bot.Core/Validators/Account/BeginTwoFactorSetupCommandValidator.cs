using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Account;

namespace Void.Bot.Core.Validators.Account;

public sealed class BeginTwoFactorSetupCommandValidator
    : FluentMessageValidator<BeginTwoFactorSetupCommand>
{
    public BeginTwoFactorSetupCommandValidator()
    {
        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required");
    }
}
