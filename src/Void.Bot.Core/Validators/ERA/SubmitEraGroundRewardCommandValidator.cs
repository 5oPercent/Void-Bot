using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;
using Void.Bot.Message.Enum.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class SubmitEraGroundRewardCommandValidator : FluentMessageValidator<SubmitEraGroundRewardCommand>
{
    public SubmitEraGroundRewardCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.MemberId).NotEmpty();
        RuleFor(command => command.AuthorityEpoch).GreaterThan(0);
        RuleFor(command => command.SourceType).Must(source => source is EraRewardSourceType.Monster or EraRewardSourceType.Chest or EraRewardSourceType.Event);
        RuleFor(command => command.SourceInstanceId).NotEmpty();
        RuleFor(command => command.SourceDefinitionId).NotEmpty().MaximumLength(128);
        RuleFor(command => command.TriggerIndex).GreaterThan(0);
        RuleFor(command => command.RewardNodeId).NotEmpty().MaximumLength(128);
        RuleFor(command => command.Placement).NotNull().Must(placement => placement != null && double.IsFinite(placement.X) &&
            double.IsFinite(placement.Y) && double.IsFinite(placement.Z) && double.IsFinite(placement.YawRadians));
    }
}
