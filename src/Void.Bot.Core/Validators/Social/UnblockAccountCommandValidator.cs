using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Validators.Social;

public class UnblockAccountCommandValidator : FluentMessageValidator<UnblockAccountCommand>
{
    public UnblockAccountCommandValidator()
    {
        RuleFor(message => message.TargetAccountId).NotEmpty();
    }
}
