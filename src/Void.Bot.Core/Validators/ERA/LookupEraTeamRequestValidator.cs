using FluentValidation;
using Void.Bot.Message.Constants.ERA;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class LookupEraTeamRequestValidator : FluentMessageValidator<LookupEraTeamRequest>
{
    public LookupEraTeamRequestValidator()
    {
        RuleFor(message => message.TeamCode).NotEmpty().Matches(EraNetworkingConstants.TeamCodePattern);
    }
}
