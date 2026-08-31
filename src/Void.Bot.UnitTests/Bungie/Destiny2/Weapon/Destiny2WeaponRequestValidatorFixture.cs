using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Core.Validators.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.UnitTest.Bungie.Destiny2.Weapon;

public class Destiny2WeaponRequestValidatorFixture
{
    private readonly Destiny2Setting _setting;

    public Destiny2WeaponRequestValidatorFixture()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:SyncLanguages:0"] = "en",
                ["Destiny2:SyncLanguages:1"] = "zh-chs"
            })
            .Build();
        _setting = new Destiny2Setting(configuration);
    }

    [Fact]
    public void PerkQueryShouldAcceptValidWeaponType()
    {
        var validator = new GetDestiny2WeaponPerksRequestValidator();

        validator.Validate(new GetDestiny2WeaponPerksRequest
        {
            WeaponTypes = [Destiny2ItemSubTypeEnum.AutoRifle],
        }).IsValid.ShouldBeTrue();

    }

    [Fact]
    public void GodRollQueryShouldRequirePerksInEveryGroup()
    {
        var validator = new FilterDestiny2WeaponGodRollRequestValidator();
        var result = validator.Validate(new FilterDestiny2WeaponGodRollRequest
        {
            WeaponTypes = [Destiny2ItemSubTypeEnum.AutoRifle],
            PerkGroups = [new Destiny2WeaponPerkGroupDto()],
        });

        result.IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GodRollQueryShouldAllowAllWeaponTypes()
    {
        var validator = new FilterDestiny2WeaponGodRollRequestValidator();

        validator.Validate(new FilterDestiny2WeaponGodRollRequest
        {
            PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [20] }],
        }).IsValid.ShouldBeTrue();
    }

    [Fact]
    public void GodRollQueryShouldRejectNoneAndUndefinedFilterValues()
    {
        var validator = new FilterDestiny2WeaponGodRollRequestValidator();

        validator.Validate(new FilterDestiny2WeaponGodRollRequest
        {
            AmmoTypes = [Destiny2AmmunitionTypeEnum.None],
            DamageTypes = [(Destiny2DamageTypeEnum)999],
            WeaponSlots = [Destiny2WeaponSlotEnum.None],
            PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [20] }],
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GodRollQueryShouldRejectInvalidExtendedFilters()
    {
        var validator = new FilterDestiny2WeaponGodRollRequestValidator();

        validator.Validate(new FilterDestiny2WeaponGodRollRequest
        {
            Rarities = [Destiny2TierTypeEnum.Unknown],
            BreakerTypes = [Destiny2BreakerTypeEnum.None],
            StatFilters =
            [
                new Destiny2WeaponStatFilterDto
                {
                    StatHash = 0,
                    MinimumValue = 100,
                    MaximumValue = 10
                }
            ],
            FoundryTraitIds = [string.Empty],
            PowerCapHashes = [0],
            ClassTypes = [(Destiny2ClassEnum)999],
            PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [20] }],
        }).IsValid.ShouldBeFalse();
    }

    [Fact]
    public void GodRollQueryShouldRejectInvalidPagination()
    {
        var validator = new FilterDestiny2WeaponGodRollRequestValidator();

        validator.Validate(new FilterDestiny2WeaponGodRollRequest
        {
            PageIndex = 0,
            PageSize = 101,
            PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [20] }],
        }).IsValid.ShouldBeFalse();
    }
}
