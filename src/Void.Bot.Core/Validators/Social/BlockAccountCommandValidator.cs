using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.Social;

namespace Void.Bot.Core.Validators.Social;

public class BlockAccountCommandValidator : FluentMessageValidator<BlockAccountCommand>
{
    public BlockAccountCommandValidator()
    {
        RuleFor(message => message.TargetAccountId).NotEmpty();
    }
}
