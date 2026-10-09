using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public class TransferEraTeamMemberCommandValidator : FluentMessageValidator<TransferEraTeamMemberCommand>
{
    public TransferEraTeamMemberCommandValidator()
    {
        RuleFor(message => message.TeamId).NotEmpty();
        RuleFor(message => message.MemberId).NotEmpty();
        RuleFor(message => message.TargetMemberId).NotEmpty().NotEqual(message => message.MemberId);
        RuleFor(message => message.AuthorityEpoch).GreaterThan(0);
    }
}
