using FluentValidation;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class JoinEraTeamCommandValidator : FluentMessageValidator<JoinEraTeamCommand>
{
    public JoinEraTeamCommandValidator()
    {
        RuleFor(message => message.TeamCode)
            .NotEmpty()
            .Matches(EraNetworkingConstants.TeamCodePattern);
        RuleFor(message => message.CharacterId)
            .NotEmpty();
    }
}
