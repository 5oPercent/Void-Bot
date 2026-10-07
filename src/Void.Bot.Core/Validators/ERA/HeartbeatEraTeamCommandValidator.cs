using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class HeartbeatEraTeamCommandValidator : FluentMessageValidator<HeartbeatEraTeamCommand>
{
    public HeartbeatEraTeamCommandValidator()
    {
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
    }
}
