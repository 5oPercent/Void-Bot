using FluentValidation;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class SwitchEraTeamCommandValidator : FluentMessageValidator<SwitchEraTeamCommand>
{
    public SwitchEraTeamCommandValidator()
    {
        RuleFor(message => message.TeamCode)
            .NotEmpty()
            .Matches(EraNetworkingConstants.TeamCodePattern);
        RuleFor(message => message.TeamId)
            .NotEmpty();
        RuleFor(message => message.MemberId)
            .NotEmpty();
        RuleFor(message => message.CharacterId)
            .NotEmpty();
    }
}
