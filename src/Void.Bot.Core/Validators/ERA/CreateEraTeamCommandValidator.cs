using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class CreateEraTeamCommandValidator : FluentMessageValidator<CreateEraTeamCommand>
{
    public CreateEraTeamCommandValidator()
    {
        RuleFor(message => message.JoinMode).IsInEnum();
        RuleFor(message => message.InvitationPermission).IsInEnum();
        RuleFor(message => message.RequestId)
            .NotEmpty();
        RuleFor(message => message.CharacterId)
            .NotEmpty();
        RuleFor(message => message.MaxMembers)
            .InclusiveBetween(2, 16);
    }
}
