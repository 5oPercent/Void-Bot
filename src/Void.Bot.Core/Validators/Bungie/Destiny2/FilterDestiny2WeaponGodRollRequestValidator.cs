using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

public class FilterDestiny2WeaponGodRollRequestValidator : FluentMessageValidator<FilterDestiny2WeaponGodRollRequest>
{
    private const int MaxWeaponTypeCount = 50;
    private const int MaxPerkGroupCount = 10;
    private const int MaxPerkCountPerGroup = 20;

    public FilterDestiny2WeaponGodRollRequestValidator(Destiny2Setting setting)
    {
        RuleFor(x => x.WeaponTypes)
            .Must(x => x.Distinct().Count() <= MaxWeaponTypeCount)
            .WithMessage($"Weapon type count must not exceed {MaxWeaponTypeCount}");

        RuleForEach(x => x.WeaponTypes)
            .NotEqual(Destiny2ItemSubTypeEnum.None)
            .WithMessage("Weapon type must not be None");

        RuleFor(x => x.PerkGroups)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one perk group is required")
            .Must(x => x.Count <= MaxPerkGroupCount)
            .WithMessage($"Perk group count must not exceed {MaxPerkGroupCount}");

        RuleForEach(x => x.PerkGroups).ChildRules(group =>
        {
            group.RuleFor(x => x.PerkHashes)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Each perk group must contain at least one perk hash")
                .Must(x => x.Distinct().Count() <= MaxPerkCountPerGroup)
                .WithMessage($"Perk hash count per group must not exceed {MaxPerkCountPerGroup}");

            group.RuleForEach(x => x.PerkHashes)
                .GreaterThan(0u)
                .WithMessage("Perk hash must be greater than zero");
        });

        RuleFor(x => x.Language)
            .Must(language => Destiny2WeaponLanguageValidation.IsSupported(language, setting))
            .WithMessage($"Language must be one of: {string.Join(", ", setting.SupportedLanguages)}");
    }
}
