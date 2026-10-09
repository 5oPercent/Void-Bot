using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class RejectEraTeamInvitationCommandValidator : FluentMessageValidator<RejectEraTeamInvitationCommand>
{
    public RejectEraTeamInvitationCommandValidator()
    {
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.InvitationId).NotEmpty();
    }
}
