using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class LeaveEraTeamCommandValidator : FluentMessageValidator<LeaveEraTeamCommand>
{
    public LeaveEraTeamCommandValidator()
    {
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
    }
}
