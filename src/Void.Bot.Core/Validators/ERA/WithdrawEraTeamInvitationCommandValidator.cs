using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class WithdrawEraTeamInvitationCommandValidator : FluentMessageValidator<WithdrawEraTeamInvitationCommand>
{
    public WithdrawEraTeamInvitationCommandValidator()
    {
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.InvitationId).NotEmpty();
    }
}
