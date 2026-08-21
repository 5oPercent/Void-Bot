using FluentValidation;
using Void.Bot.Core.Middlewares.FluentMessageValidator;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Validators.Bungie.Destiny2;

public class FilterDestiny2WeaponGodRollRequestValidator : FluentMessageValidator<FilterDestiny2WeaponGodRollRequest>
{
    private const int MaxWeaponTypeCount = 50;
    private const int MaxFilterValueCount = 10;
    private const int MaxStatFilterCount = 50;
    private const int MaxFoundryFilterCount = 20;
    private const int MaxPerkGroupCount = 10;
    private const int MaxPerkCountPerGroup = 20;

    public FilterDestiny2WeaponGodRollRequestValidator(Destiny2Setting setting)
    {
        RuleFor(x => x.PageIndex)
            .GreaterThanOrEqualTo(1)
            .WithMessage("Page index must be greater than or equal to 1");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("Page size must be between 1 and 100");

        RuleFor(x => x.WeaponTypes)
            .Must(x => x.Distinct().Count() <= MaxWeaponTypeCount)
            .WithMessage($"Weapon type count must not exceed {MaxWeaponTypeCount}");

        RuleForEach(x => x.WeaponTypes)
            .NotEqual(Destiny2ItemSubTypeEnum.None)
            .WithMessage("Weapon type must not be None");

        RuleFor(x => x.AmmoTypes)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Ammo type count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.AmmoTypes)
            .IsInEnum()
            .NotEqual(Destiny2AmmunitionTypeEnum.None)
            .WithMessage("Ammo type must be a defined value other than None");

        RuleFor(x => x.DamageTypes)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Damage type count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.DamageTypes)
            .IsInEnum()
            .NotEqual(Destiny2DamageTypeEnum.None)
            .WithMessage("Damage type must be a defined value other than None");

        RuleFor(x => x.WeaponSlots)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Weapon slot count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.WeaponSlots)
            .IsInEnum()
            .NotEqual(Destiny2WeaponSlotEnum.None)
            .WithMessage("Weapon slot must be a defined value other than None");

        RuleFor(x => x.Rarities)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Rarity count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.Rarities)
            .IsInEnum()
            .NotEqual(Destiny2TierTypeEnum.Unknown)
            .WithMessage("Rarity must be a defined value other than Unknown");

        RuleFor(x => x.BreakerTypes)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Breaker type count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.BreakerTypes)
            .IsInEnum()
            .NotEqual(Destiny2BreakerTypeEnum.None)
            .WithMessage("Breaker type must be a defined value other than None");

        RuleFor(x => x.StatFilters)
            .Must(x => x.Count <= MaxStatFilterCount)
            .WithMessage($"Stat filter count must not exceed {MaxStatFilterCount}");

        RuleForEach(x => x.StatFilters).ChildRules(stat =>
        {
            stat.RuleFor(x => x.StatHash)
                .GreaterThan(0u)
                .WithMessage("Stat hash must be greater than zero");
            stat.RuleFor(x => x)
                .Must(x => !x.MinimumValue.HasValue || !x.MaximumValue.HasValue ||
                           x.MinimumValue.Value <= x.MaximumValue.Value)
                .WithMessage("Stat minimum value must not exceed maximum value");
        });

        RuleFor(x => x.FoundryTraitIds)
            .Must(x => x.Distinct(StringComparer.OrdinalIgnoreCase).Count() <= MaxFoundryFilterCount)
            .WithMessage($"Foundry filter count must not exceed {MaxFoundryFilterCount}");

        RuleForEach(x => x.FoundryTraitIds)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.CurrentVersions)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Current version count must not exceed {MaxFilterValueCount}");

        RuleFor(x => x.PowerCapHashes)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Power cap hash count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.PowerCapHashes)
            .GreaterThan(0u)
            .WithMessage("Power cap hash must be greater than zero");

        RuleFor(x => x.ClassTypes)
            .Must(x => x.Distinct().Count() <= MaxFilterValueCount)
            .WithMessage($"Class type count must not exceed {MaxFilterValueCount}");

        RuleForEach(x => x.ClassTypes)
            .IsInEnum()
            .WithMessage("Class type must be a defined value");

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
