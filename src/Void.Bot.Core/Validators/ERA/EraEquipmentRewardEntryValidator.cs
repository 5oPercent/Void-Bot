using FluentValidation;
using Void.Bot.Message.Dto.ERA;

namespace Void.Bot.Core.Validators.ERA;

public sealed class EraEquipmentRewardEntryValidator : AbstractValidator<EraEquipmentRewardEntry>
{
    public EraEquipmentRewardEntryValidator()
    {
        RuleFor(entry => entry.TemplateId).NotEmpty();
        RuleFor(entry => entry.Grade).IsInEnum();
        RuleFor(entry => entry.Count).InclusiveBetween(1, 120);
    }
}
