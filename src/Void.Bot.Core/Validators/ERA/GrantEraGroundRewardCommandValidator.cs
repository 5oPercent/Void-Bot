using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Commands.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class GrantEraGroundRewardCommandValidator : FluentMessageValidator<GrantEraGroundRewardCommand>
{
    public GrantEraGroundRewardCommandValidator()
    {
        RuleFor(command => command.WorldInstanceId).NotEmpty();
        RuleFor(command => command.RequestId).NotEmpty();
        RuleFor(command => command.CharacterId).NotEmpty();
        RuleFor(command => command.MemberId).NotEmpty();
        RuleFor(command => command.AuthorityEpoch).GreaterThan(0);
        RuleFor(command => command.Entries).NotEmpty().Must(entries => entries != null && entries.Count <= 120 &&
            entries.All(entry => entry != null) && entries.Sum(entry => (long)entry.Count) <= 120);
        RuleForEach(command => command.Entries).NotNull().SetValidator(new EraEquipmentRewardEntryValidator());
        RuleFor(command => command.Placement).NotNull().Must(placement => placement != null && double.IsFinite(placement.X) &&
            double.IsFinite(placement.Y) && double.IsFinite(placement.Z) && double.IsFinite(placement.YawRadians));
    }
}
