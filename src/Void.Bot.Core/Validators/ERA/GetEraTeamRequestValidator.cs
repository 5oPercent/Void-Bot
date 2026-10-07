using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraTeamRequestValidator : FluentMessageValidator<GetEraTeamRequest>
{
    public GetEraTeamRequestValidator()
    {
        RuleFor(message => message.TeamId)
            .NotEmpty();
    }
}
