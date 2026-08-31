using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Message.Request.Bungie.Destiny2;

/// <summary>
/// Filters weapons that satisfy every requested Perk Group on distinct sockets.
/// Empty collection filters and nullable filters without a value do not restrict their dimensions.
/// </summary>
public class FilterDestiny2WeaponGodRollRequest : PageInputDto, IRequest
{
    public List<Destiny2ItemSubTypeEnum> WeaponTypes { get; set; } = [];

    public List<Destiny2AmmunitionTypeEnum> AmmoTypes { get; set; } = [];

    public List<Destiny2DamageTypeEnum> DamageTypes { get; set; } = [];

    public List<Destiny2WeaponSlotEnum> WeaponSlots { get; set; } = [];

    public List<Destiny2TierTypeEnum> Rarities { get; set; } = [];

    public List<Destiny2BreakerTypeEnum> BreakerTypes { get; set; } = [];

    public bool? IsCraftable { get; set; }

    public List<Destiny2WeaponStatFilterDto> StatFilters { get; set; } = [];

    public List<string> FoundryTraitIds { get; set; } = [];

    public List<uint> CurrentVersions { get; set; } = [];

    public List<uint> PowerCapHashes { get; set; } = [];

    public bool? IsAdept { get; set; }

    public bool? IsHolofoil { get; set; }

    public List<Destiny2ClassEnum> ClassTypes { get; set; } = [];

    public List<Destiny2WeaponPerkGroupDto> PerkGroups { get; set; } = [];

}

/// <summary>
/// God Roll weapon filter response.
/// </summary>
public class FilterDestiny2WeaponGodRollResponse : VoidBotResponse<PagedListDto<Destiny2WeaponGodRollDto>>
{
}
