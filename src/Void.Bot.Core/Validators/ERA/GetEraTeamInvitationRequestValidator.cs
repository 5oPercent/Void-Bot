using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Request.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class GetEraTeamInvitationRequestValidator : FluentMessageValidator<GetEraTeamInvitationRequest>
{
    public GetEraTeamInvitationRequestValidator()
    {
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.InvitationId).NotEmpty();
    }
}
