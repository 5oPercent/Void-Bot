using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class UpdateEraTeamSettingsCommandValidator : FluentMessageValidator<UpdateEraTeamSettingsCommand>
{
    public UpdateEraTeamSettingsCommandValidator()
    {
        RuleFor(message => message.ExpectedVersion).InclusiveBetween(0, long.MaxValue - 1);
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.MemberId).NotEmpty();
        RuleFor(message => message.JoinMode).IsInEnum();
        RuleFor(message => message.InvitationPermission).IsInEnum();
    }
}
