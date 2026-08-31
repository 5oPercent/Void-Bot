using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

public class GetDestiny2WeaponPerksRequestValidator : FluentMessageValidator<GetDestiny2WeaponPerksRequest>
{
    private const int MaxWeaponTypeCount = 50;

    public GetDestiny2WeaponPerksRequestValidator()
    {
        RuleFor(x => x.WeaponTypes)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one weapon type is required")
            .Must(x => x.Distinct().Count() <= MaxWeaponTypeCount)
            .WithMessage($"Weapon type count must not exceed {MaxWeaponTypeCount}");

        RuleForEach(x => x.WeaponTypes)
            .NotEqual(Destiny2ItemSubTypeEnum.None)
            .WithMessage("Weapon type must not be None");

    }
}
