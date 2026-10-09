using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class SendEraTeamInvitationCommandValidator : FluentMessageValidator<SendEraTeamInvitationCommand>
{
    public SendEraTeamInvitationCommandValidator()
    {
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.MemberId).NotEmpty();
        RuleFor(message => message.TargetAccountId).NotEmpty();
        RuleFor(message => message.RequestId).NotEmpty();
    }
}
