using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class JoinEraFriendTeamCommandValidator : FluentMessageValidator<JoinEraFriendTeamCommand>
{
    public JoinEraFriendTeamCommandValidator()
    {
        RuleFor(message => message).Must(message =>
            message.SourceTeamId.HasValue == message.SourceMemberId.HasValue &&
            (!message.SourceTeamId.HasValue ||
             (message.SourceTeamId.Value != Guid.Empty && message.SourceMemberId!.Value != Guid.Empty)));
        RuleFor(message => message.FriendAccountId).NotEmpty();
        RuleFor(message => message.ExpectedTeamId).NotEmpty();
        RuleFor(message => message.CharacterId).NotEmpty();
    }
}
