using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class AcceptEraTeamInvitationCommandValidator : FluentMessageValidator<AcceptEraTeamInvitationCommand>
{
    public AcceptEraTeamInvitationCommandValidator()
    {
        RuleFor(message => message).Must(message =>
            message.SourceTeamId.HasValue == message.SourceMemberId.HasValue &&
            (!message.SourceTeamId.HasValue ||
             (message.SourceTeamId.Value != Guid.Empty && message.SourceMemberId!.Value != Guid.Empty)));
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.InvitationId).NotEmpty();
        RuleFor(message => message.CharacterId).NotEmpty();
    }
}
