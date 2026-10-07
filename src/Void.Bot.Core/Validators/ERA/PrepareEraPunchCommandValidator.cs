using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class PrepareEraPunchCommandValidator : FluentMessageValidator<PrepareEraPunchCommand>
{
    public PrepareEraPunchCommandValidator()
    {
        RuleFor(message => message.AuthorityEpoch)
            .GreaterThan(0);
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
    }
}
