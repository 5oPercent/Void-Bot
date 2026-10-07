using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class CompleteEraHostMigrationCommandValidator : FluentMessageValidator<CompleteEraHostMigrationCommand>
{
    public CompleteEraHostMigrationCommandValidator()
    {
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
        RuleFor(message => message.AuthorityEpoch)
            .GreaterThan(0);
    }
}
