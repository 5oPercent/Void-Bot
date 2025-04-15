using System.Text.Json.Serialization;

namespace Void.Bot.Message.Dto.Bungie.Destiny2;

public class Destiny2LanguageComponentContentDto
{
    public Destiny2ManifestComponentContentPathDto? EN { get; set; }

    public Destiny2ManifestComponentContentPathDto? FR { get; set; }

    public Destiny2ManifestComponentContentPathDto? ES { get; set; }

    [JsonPropertyName("es-mx")]
    public Destiny2ManifestComponentContentPathDto? ES_MX { get; set; }

    public Destiny2ManifestComponentContentPathDto? DE { get; set; }

    public Destiny2ManifestComponentContentPathDto? IT { get; set; }

    public Destiny2ManifestComponentContentPathDto? JA { get; set; }

    [JsonPropertyName("pt-br")]
    public Destiny2ManifestComponentContentPathDto? PT_BR { get; set; }

    public Destiny2ManifestComponentContentPathDto? RU { get; set; }

    public Destiny2ManifestComponentContentPathDto? PL { get; set; }

    public Destiny2ManifestComponentContentPathDto? KO { get; set; }

    [JsonPropertyName("zh-cht")]
    public Destiny2ManifestComponentContentPathDto? ZH_CHT { get; set; }

    [JsonPropertyName("zh-chs")]
    public Destiny2ManifestComponentContentPathDto? ZH_CHS { get; set; }
}

public class Destiny2ManifestComponentContentPathDto
{
    public string DestinyArtDyeChannelDefinition { get; set; } = string.Empty;
    public string DestinyArtDyeReferenceDefinition { get; set; } = string.Empty;
    public string DestinyPlaceDefinition { get; set; } = string.Empty;
    public string DestinyActivityDefinition { get; set; } = string.Empty;
    public string DestinyActivityTypeDefinition { get; set; } = string.Empty;
    public string DestinyClassDefinition { get; set; } = string.Empty;
    public string DestinyGenderDefinition { get; set; } = string.Empty;
    public string DestinyInventoryBucketDefinition { get; set; } = string.Empty;
    public string DestinyRaceDefinition { get; set; } = string.Empty;
    public string DestinyUnlockDefinition { get; set; } = string.Empty;
    public string DestinyStatGroupDefinition { get; set; } = string.Empty;
    public string DestinyProgressionMappingDefinition { get; set; } = string.Empty;
    public string DestinyFactionDefinition { get; set; } = string.Empty;
    public string DestinyVendorGroupDefinition { get; set; } = string.Empty;
    public string DestinyRewardSourceDefinition { get; set; } = string.Empty;
    public string DestinyUnlockValueDefinition { get; set; } = string.Empty;
    public string DestinyRewardMappingDefinition { get; set; } = string.Empty;
    public string DestinyRewardSheetDefinition { get; set; } = string.Empty;
    public string DestinyItemCategoryDefinition { get; set; } = string.Empty;
    public string DestinyDamageTypeDefinition { get; set; } = string.Empty;
    public string DestinyActivityModeDefinition { get; set; } = string.Empty;
    public string DestinyMedalTierDefinition { get; set; } = string.Empty;
    public string DestinyAchievementDefinition { get; set; } = string.Empty;
    public string DestinyActivityGraphDefinition { get; set; } = string.Empty;
    public string DestinyActivityInteractableDefinition { get; set; } = string.Empty;
    public string DestinyBondDefinition { get; set; } = string.Empty;
    public string DestinyCharacterCustomizationCategoryDefinition { get; set; } = string.Empty;
    public string DestinyCharacterCustomizationOptionDefinition { get; set; } = string.Empty;
    public string DestinyCollectibleDefinition { get; set; } = string.Empty;
    public string DestinyDestinationDefinition { get; set; } = string.Empty;
    public string DestinyEntitlementOfferDefinition { get; set; } = string.Empty;
    public string DestinyEquipmentSlotDefinition { get; set; } = string.Empty;
    public string DestinyEventCardDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderActivityGraphDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderActivitySetDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderLabelDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderLabelGroupDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderOptionDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderOptionGroupDefinition { get; set; } = string.Empty;
    public string DestinyStatDefinition { get; set; } = string.Empty;
    public string DestinyInventoryItemDefinition { get; set; } = string.Empty;
    public string DestinyInventoryItemLiteDefinition { get; set; } = string.Empty;
    public string DestinyItemTierTypeDefinition { get; set; } = string.Empty;
    public string DestinyLoadoutColorDefinition { get; set; } = string.Empty;
    public string DestinyLoadoutIconDefinition { get; set; } = string.Empty;
    public string DestinyLoadoutNameDefinition { get; set; } = string.Empty;
    public string DestinyLocationDefinition { get; set; } = string.Empty;
    public string DestinyLoreDefinition { get; set; } = string.Empty;
    public string DestinyMaterialRequirementSetDefinition { get; set; } = string.Empty;
    public string DestinyMetricDefinition { get; set; } = string.Empty;
    public string DestinyObjectiveDefinition { get; set; } = string.Empty;
    public string DestinySandboxPerkDefinition { get; set; } = string.Empty;
    public string DestinyPlatformBucketMappingDefinition { get; set; } = string.Empty;
    public string DestinyPlugSetDefinition { get; set; } = string.Empty;
    public string DestinyPowerCapDefinition { get; set; } = string.Empty;
    public string DestinyPresentationNodeDefinition { get; set; } = string.Empty;
    public string DestinyProgressionDefinition { get; set; } = string.Empty;
    public string DestinyProgressionLevelRequirementDefinition { get; set; } = string.Empty;
    public string DestinyRecordDefinition { get; set; } = string.Empty;
    public string DestinyRewardAdjusterPointerDefinition { get; set; } = string.Empty;
    public string DestinyRewardAdjusterProgressionMapDefinition { get; set; } = string.Empty;
    public string DestinyRewardItemListDefinition { get; set; } = string.Empty;
    public string DestinySackRewardItemListDefinition { get; set; } = string.Empty;
    public string DestinySandboxPatternDefinition { get; set; } = string.Empty;
    public string DestinySeasonDefinition { get; set; } = string.Empty;
    public string DestinySeasonPassDefinition { get; set; } = string.Empty;
    public string DestinySocialCommendationDefinition { get; set; } = string.Empty;
    public string DestinySocketCategoryDefinition { get; set; } = string.Empty;
    public string DestinySocketTypeDefinition { get; set; } = string.Empty;
    public string DestinyTraitDefinition { get; set; } = string.Empty;
    public string DestinyUnlockCountMappingDefinition { get; set; } = string.Empty;
    public string DestinyUnlockEventDefinition { get; set; } = string.Empty;
    public string DestinyUnlockExpressionMappingDefinition { get; set; } = string.Empty;
    public string DestinyVendorDefinition { get; set; } = string.Empty;
    public string DestinyMilestoneDefinition { get; set; } = string.Empty;
    public string DestinyActivityModifierDefinition { get; set; } = string.Empty;
    public string DestinyReportReasonCategoryDefinition { get; set; } = string.Empty;
    public string DestinyArtifactDefinition { get; set; } = string.Empty;
    public string DestinyBreakerTypeDefinition { get; set; } = string.Empty;
    public string DestinyChecklistDefinition { get; set; } = string.Empty;
    public string DestinyEnergyTypeDefinition { get; set; } = string.Empty;
    public string DestinySocialCommendationNodeDefinition { get; set; } = string.Empty;
    public string DestinyGuardianRankDefinition { get; set; } = string.Empty;
    public string DestinyGuardianRankConstantsDefinition { get; set; } = string.Empty;
    public string DestinyLoadoutConstantsDefinition { get; set; } = string.Empty;
    public string DestinyFireteamFinderConstantsDefinition { get; set; } = string.Empty;
    public string DestinyGlobalConstantsDefinition { get; set; } = string.Empty;
}