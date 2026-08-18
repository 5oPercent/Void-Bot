using Shouldly;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;

namespace Void.Bot.UnitTest.Bungie.Destiny2.Weapon;

public class Destiny2WeaponSeasonResolutionFixture
{
    private const string InventoryItemJson = """
        {
          "10": {
            "hash": 10,
            "itemType": 19,
            "displayProperties": { "name": "Adaptive Frame", "icon": "/frame-10.png" },
            "plug": { "plugCategoryIdentifier": "intrinsics" }
          },
          "20": {
            "hash": 20,
            "itemType": 19,
            "displayProperties": { "name": "Demolitionist", "icon": "/perk-20.png" },
            "plug": { "plugCategoryIdentifier": "traits" }
          },
          "30": {
            "hash": 30,
            "itemType": 19,
            "displayProperties": { "name": "Infusion", "icon": "" },
            "plug": { "plugCategoryIdentifier": "dummy_infuse" }
          },
          "100": {
            "hash": 100,
            "seasonHash": 900,
            "traitIds": [ "weapon_type_auto_rifle", "releases.v700.season" ],
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "Direct SeasonHash Rifle", "icon": "/weapon-100.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 }
              ]
            }
          },
          "101": {
            "hash": 101,
            "traitIds": [ "weapon_type_auto_rifle", "releases.v600.season" ],
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "Release Trait Rifle", "icon": "/weapon-101.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 }
              ]
            }
          },
          "102": {
            "hash": 102,
            "traitIds": [ "weapon_type_auto_rifle", "releases.v540.season" ],
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "Season Missing From Table Rifle", "icon": "/weapon-102.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 }
              ]
            }
          },
          "103": {
            "hash": 103,
            "traitIds": [ "weapon_type_auto_rifle", "releases.v999.unknown" ],
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "Unknown Trait Rifle", "icon": "/weapon-103.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 }
              ]
            }
          },
          "104": {
            "hash": 104,
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "No Trait Rifle", "icon": "/weapon-104.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 }
              ]
            }
          }
        }
        """;

    private const string SeasonJson = """
        {
          "900": {
            "hash": 900,
            "seasonNumber": 20,
            "displayProperties": { "name": "Season of Defiance" }
          },
          "916": {
            "hash": 916,
            "seasonNumber": 16,
            "displayProperties": { "name": "Season of the Risen" }
          },
          "925": {
            "hash": 925,
            "seasonNumber": 25,
            "displayProperties": { "name": "Episode: Revenant" }
          }
        }
        """;

    private const string PlugSetJson = """
        {
          "500": {
            "hash": 500,
            "reusablePlugItems": [ { "plugItemHash": 20, "currentlyCanRoll": true } ]
          }
        }
        """;

    [Fact]
    public void SeasonHashShouldTakePriorityOverReleaseTrait()
    {
        var rollData = BuildRollData();
        var item = rollData.Items[100];

        item.SeasonHash.ShouldBe(900u);
        item.SeasonNumber.ShouldBe(20);
        item.SeasonName.ShouldBe("Season of Defiance");
    }

    [Fact]
    public void ReleaseTraitShouldResolveSeasonWhenSeasonHashIsMissing()
    {
        var rollData = BuildRollData();
        var item = rollData.Items[101];

        item.SeasonHash.ShouldBe(916u);
        item.SeasonNumber.ShouldBe(16);
        item.SeasonName.ShouldBe("Season of the Risen");
    }

    [Fact]
    public void ReleaseTraitShouldResolveSeasonNumberWhenSeasonIsMissingFromManifest()
    {
        var rollData = BuildRollData();
        var item = rollData.Items[102];

        item.SeasonHash.ShouldBe(0u);
        item.SeasonNumber.ShouldBe(15);
        item.SeasonName.ShouldBeEmpty();
    }

    [Fact]
    public void UnknownReleaseTraitShouldLeaveSeasonEmpty()
    {
        var rollData = BuildRollData();
        var item = rollData.Items[103];

        item.SeasonHash.ShouldBe(0u);
        item.SeasonNumber.ShouldBe(0);
        item.SeasonName.ShouldBeEmpty();
    }

    [Fact]
    public void MissingReleaseTraitShouldLeaveSeasonEmpty()
    {
        var rollData = BuildRollData();
        var item = rollData.Items[104];

        item.SeasonHash.ShouldBe(0u);
        item.SeasonNumber.ShouldBe(0);
        item.SeasonName.ShouldBeEmpty();
    }

    [Fact]
    public void MappingTableShouldCoverAllKnownReleaseTraits()
    {
        var known = new[]
        {
            "releases.v300.annual", "releases.v310.season", "releases.v320.season",
            "releases.v400.annual", "releases.v400.season", "releases.v410.season",
            "releases.v420.season", "releases.v450.season", "releases.v460.season",
            "releases.v470.season", "releases.v480.season", "releases.v490.season",
            "releases.v500.annual", "releases.v500.season", "releases.v510.season",
            "releases.v520.season", "releases.v530.season", "releases.v540.season",
            "releases.v600.annual", "releases.v600.season", "releases.v610.season",
            "releases.v620.season", "releases.v630.season",
            "releases.v700.annual", "releases.v700.season", "releases.v710.season",
            "releases.v720.season", "releases.v730.season",
            "releases.v800.annual", "releases.v800.season", "releases.v810.season",
            "releases.v820.season",
            "releases.v900.core", "releases.v900.dlc", "releases.v910", "releases.v910.core",
            "releases.v950", "releases.v950.core", "releases.v950.dlc",
            "releases.v960.core", "releases.v970.core"
        };

        foreach (var trait in known)
            Destiny2WeaponReleaseSeasonMap.TryResolveSeason(trait, out var seasonNumber).ShouldBeTrue(trait);

        Destiny2WeaponReleaseSeasonMap.TryResolveSeason("releases.v999.unknown", out _).ShouldBeFalse();
        Destiny2WeaponReleaseSeasonMap.TryResolveSeason("weapon_type_auto_rifle", out _).ShouldBeFalse();
        Destiny2WeaponReleaseSeasonMap.TryResolveSeason("", out _).ShouldBeFalse();
    }

    private static Destiny2WeaponRollDataDto BuildRollData()
        => Destiny2WeaponDataProvider.BuildRollData(InventoryItemJson, PlugSetJson, SeasonJson);
}
