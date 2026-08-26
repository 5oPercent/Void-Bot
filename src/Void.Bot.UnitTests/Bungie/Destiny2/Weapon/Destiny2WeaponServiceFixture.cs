using System.Linq.Expressions;
using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Shouldly;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
using Void.Bot.Core.Services.Jobs;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.UnitTest.Bungie.Destiny2.Weapon;

public class Destiny2WeaponServiceFixture
{
    internal const string InventoryItemJson = """
        {
          "10": {
            "hash": 10,
            "itemType": 19,
            "displayProperties": { "name": "Adaptive Frame", "description": "Balanced frame", "icon": "/frame-10.png" },
            "perks": [ { "perkHash": 600 } ],
            "plug": { "plugCategoryIdentifier": "intrinsics" }
          },
          "11": {
            "hash": 11,
            "itemType": 19,
            "displayProperties": { "name": "Rapid-Fire Frame", "description": "Fast frame", "icon": "/frame-11.png" },
            "plug": { "plugCategoryIdentifier": "intrinsics" }
          },
          "20": {
            "hash": 20,
            "itemType": 19,
            "displayProperties": { "name": "Demolitionist", "description": "Grenade energy", "icon": "/perk-20.png" },
            "plug": { "plugCategoryIdentifier": "traits" }
          },
          "21": {
            "hash": 21,
            "itemType": 19,
            "displayProperties": { "name": "Unavailable Perk", "description": "Cannot roll", "icon": "/perk-21.png" },
            "plug": { "plugCategoryIdentifier": "traits" }
          },
          "22": {
            "hash": 22,
            "itemType": 19,
            "displayProperties": { "name": "Frenzy", "description": "Increases damage", "icon": "/perk-22.png" },
            "plug": { "plugCategoryIdentifier": "traits" }
          },
          "23": {
            "hash": 23,
            "itemType": 19,
            "displayProperties": { "name": "Quickdraw", "description": "Draw faster", "icon": "/perk-23.png" },
            "plug": { "plugCategoryIdentifier": "traits" }
          },
          "24": {
            "hash": 24,
            "itemType": 19,
            "displayProperties": { "name": "Demolitionist", "description": "Improved grenade energy", "icon": "/perk-20.png" },
            "plug": { "plugCategoryIdentifier": "enhanced_traits" }
          },
          "30": {
            "hash": 30,
            "itemType": 19,
            "displayProperties": { "name": "Infusion", "description": "Infusion placeholder", "icon": "" },
            "plug": { "plugCategoryIdentifier": "dummy_infuse" }
          },
          "100": {
            "hash": 100,
            "seasonHash": 900,
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "defaultDamageType": 1,
            "classType": 0,
            "isAdept": true,
            "isHolofoil": false,
            "inventory": { "tierType": 5, "tierTypeName": "Legendary", "recipeItemHash": 700 },
            "equippingBlock": { "ammoType": 1, "equipmentSlotTypeHash": 1498876634 },
            "traitIds": [ "item.weapon.foundry.suros" ],
            "traitHashes": [ 800 ],
            "quality": {
              "currentVersion": 1,
              "infusionCategoryHash": 710,
              "infusionCategoryName": "Auto Rifle",
              "versions": [ { "powerCapHash": 720 } ],
              "displayVersionWatermarkIcons": [ "/watermark-v1.png" ]
            },
            "screenshot": "/screenshot-100.jpg",
            "iconWatermark": "/watermark-100.png",
            "iconWatermarkShelved": "/watermark-shelved-100.png",
            "secondaryIcon": "/secondary-100.png",
            "secondaryOverlay": "/overlay-100.png",
            "secondarySpecial": "/special-100.png",
            "backgroundColor": { "red": 10, "green": 20, "blue": 30, "alpha": 255 },
            "displayProperties": { "name": "Test Rifle", "description": "A test weapon", "icon": "/weapon-100.png" },
            "stats": {
              "stats": {
                "4284893193": { "statHash": 4284893193, "value": 600, "minimum": 0, "maximum": 1000, "displayMaximum": 1000 },
                "12345": { "statHash": 12345, "value": 75, "minimum": 0, "maximum": 100, "displayMaximum": 100 }
              }
            },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 10 },
                { "randomizedPlugSetHash": 500 },
                { "randomizedPlugSetHash": 501 }
              ]
            }
          },
          "101": {
            "hash": 101,
            "seasonHash": 901,
            "itemType": 3,
            "itemSubType": 24,
            "itemTypeDisplayName": "Submachine Gun",
            "defaultDamageType": 2,
            "breakerType": 2,
            "breakerTypeHash": 301,
            "classType": 1,
            "isAdept": false,
            "isHolofoil": true,
            "inventory": { "tierType": 6, "tierTypeName": "Exotic", "recipeItemHash": 0 },
            "equippingBlock": { "ammoType": 2, "equipmentSlotTypeHash": 2465295065 },
            "traitIds": [ "item.weapon.foundry.omolon" ],
            "traitHashes": [ 801 ],
            "displayProperties": { "name": "Test SMG", "description": "Another test weapon", "icon": "/weapon-101.png" },
            "sockets": {
              "intrinsicSockets": [ { "plugItemHash": 30 } ],
              "socketEntries": [
                { "singleInitialItemHash": 11 },
                { "randomizedPlugSetHash": 502 }
              ]
            }
          },
          "102": {
            "hash": 102,
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "redacted": true,
            "displayProperties": { "name": "Hidden Weapon", "description": "Hidden", "icon": "/weapon-102.png" },
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

    internal const string SeasonJson = """
        {
          "900": {
            "hash": 900,
            "seasonNumber": 23,
            "displayProperties": { "name": "Season of the Wish" }
          },
          "901": {
            "hash": 901,
            "seasonNumber": 24,
            "displayProperties": { "name": "Episode: Echoes" }
          }
        }
        """;

    internal const string PlugSetJson = """
        {
          "500": {
            "hash": 500,
            "reusablePlugItems": [
              { "plugItemHash": 20, "currentlyCanRoll": true },
              { "plugItemHash": 21, "currentlyCanRoll": false }
            ]
          },
          "501": {
            "hash": 501,
            "reusablePlugItems": [ { "plugItemHash": 22, "currentlyCanRoll": true } ]
          },
          "502": {
            "hash": 502,
            "reusablePlugItems": [
              { "plugItemHash": 23, "currentlyCanRoll": true },
              { "plugItemHash": 24, "currentlyCanRoll": true }
            ]
          }
        }
        """;

    internal const string StatJson = """
        {
          "4284893193": {
            "hash": 4284893193,
            "displayProperties": { "name": "Rounds Per Minute", "description": "Fire rate", "icon": "/rpm.png" }
          },
          "12345": {
            "hash": 12345,
            "displayProperties": { "name": "Range", "description": "Effective range", "icon": "/range.png" }
          }
        }
        """;

    internal const string TraitJson = """
        {
          "800": {
            "hash": 800,
            "displayProperties": { "name": "SUROS", "description": "SUROS foundry", "icon": "/suros.png" }
          },
          "801": {
            "hash": 801,
            "displayProperties": { "name": "Omolon", "description": "Omolon foundry", "icon": "/omolon.png" }
          }
        }
        """;

    internal const string BreakerTypeJson = """
        {
          "300": {
            "hash": 300,
            "enumValue": 1,
            "displayProperties": { "name": "Barrier Champions", "description": "Pierces Barrier Champion shields", "icon": "/barrier.png" }
          },
          "301": {
            "hash": 301,
            "enumValue": 2,
            "displayProperties": { "name": "Overload Champions", "description": "Disrupts Overload Champions", "icon": "/overload.png" }
          }
        }
        """;

    internal const string SandboxPerkJson = """
        {
          "600": {
            "hash": 600,
            "displayProperties": { "name": "\uE070Barrier", "description": "Frame breaker perk", "icon": "/frame-barrier.png" }
          }
        }
        """;

    private readonly Destiny2WeaponRollDataDto _rollData;
    private readonly IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> _weaponTypeData;
    private readonly FakeWeaponDataProvider _dataProvider;
    private readonly Destiny2WeaponService _service;

    public Destiny2WeaponServiceFixture()
    {
        _rollData = Destiny2WeaponDataProvider.BuildRollData(
            InventoryItemJson,
            PlugSetJson,
            SeasonJson,
            StatJson,
            TraitJson,
            BreakerTypeJson,
            SandboxPerkJson);
        _weaponTypeData = Destiny2WeaponDataProvider.BuildWeaponTypeData(InventoryItemJson);
        _dataProvider = new FakeWeaponDataProvider(_weaponTypeData, _rollData);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Destiny2:SyncLanguages:0"] = "en"
            })
            .Build();
        _service = new Destiny2WeaponService(
            _dataProvider,
            new Destiny2Setting(configuration),
            new FakeBackgroundJobClient());
    }

    [Fact]
    public void DataProviderShouldBuildWeaponTypesWeaponsAndCurrentPerks()
    {
        _weaponTypeData.Keys.Order().ShouldBe([
            Destiny2ItemSubTypeEnum.AutoRifle,
            Destiny2ItemSubTypeEnum.SubmachineGun
        ]);
        _rollData.WeaponTypes.Keys.Order().ShouldBe([
            Destiny2ItemSubTypeEnum.AutoRifle,
            Destiny2ItemSubTypeEnum.SubmachineGun
        ]);
        _rollData.Weapons.Keys.Order().ShouldBe([100u, 101u]);
        _rollData.Weapons[100].FrameHash.ShouldBe(10u);
        _rollData.Weapons[100].RoundsPerMinute.ShouldBe(600);
        _rollData.Weapons[100].AmmoType.ShouldBe(Destiny2AmmunitionTypeEnum.Primary);
        _rollData.Weapons[100].DamageType.ShouldBe(Destiny2DamageTypeEnum.Kinetic);
        _rollData.Weapons[100].WeaponSlot.ShouldBe(Destiny2WeaponSlotEnum.Kinetic);
        _rollData.Weapons[100].Rarity.ShouldBe(Destiny2TierTypeEnum.Superior);
        _rollData.Weapons[100].BreakerType.ShouldBe(Destiny2BreakerTypeEnum.ShieldPiercing);
        _rollData.Weapons[100].Breaker.Hash.ShouldBe(300u);
        _rollData.Weapons[100].Breaker.Name.ShouldBe("Barrier Champions");
        _rollData.Weapons[100].Breaker.Icon.ShouldBe("/barrier.png");
        _rollData.Weapons[101].BreakerType.ShouldBe(Destiny2BreakerTypeEnum.Disruption);
        _rollData.Weapons[101].Breaker.Hash.ShouldBe(301u);
        _rollData.Weapons[100].IsCraftable.ShouldBeTrue();
        _rollData.Weapons[100].RecipeItemHash.ShouldBe(700u);
        _rollData.Weapons[100].Stats.Single(stat => stat.StatHash == 12345).Name.ShouldBe("Range");
        _rollData.Weapons[100].Foundries.Single().Name.ShouldBe("SUROS");
        _rollData.Weapons[100].Version.CurrentVersion.ShouldBe(1u);
        _rollData.Weapons[100].IsAdept.ShouldBeTrue();
        _rollData.Weapons[100].ClassType.ShouldBe(Destiny2ClassEnum.Titan);
        _rollData.Weapons[100].PerkSockets[0].PerkHashes.ShouldContain(20u);
        _rollData.Weapons[100].PerkSockets[0].PerkHashes.ShouldNotContain(21u);
        _rollData.PerkVariantGroups[20].Order().ShouldBe([20u, 24u]);
        _rollData.PerkVariantGroups[24].Order().ShouldBe([20u, 24u]);
    }

    [Fact]
    public async Task DataProviderAndFilterShouldKeepBreakerWeaponsWithoutRandomPerkSockets()
    {
        const string inventoryItemJson = """
            {
              "200": {
                "hash": 200,
                "itemType": 3,
                "itemSubType": 13,
                "itemTypeDisplayName": "Pulse Rifle",
                "breakerType": 1,
                "breakerTypeHash": 300,
                "displayProperties": { "name": "Fixed Exotic", "description": "Fixed perks", "icon": "/fixed.png" },
                "sockets": {
                  "socketEntries": [ { "singleInitialItemHash": 10 } ]
                }
              },
              "201": {
                "hash": 201,
                "itemType": 3,
                "itemSubType": 8,
                "itemTypeDisplayName": "Hand Cannon",
                "breakerType": 2,
                "breakerTypeHash": 301,
                "displayProperties": { "name": "Socketless Exotic", "description": "No sockets", "icon": "/socketless.png" }
              },
              "10": {
                "hash": 10,
                "itemType": 19,
                "displayProperties": { "name": "Exotic Intrinsic", "description": "Intrinsic perk", "icon": "/intrinsic.png" },
                "plug": { "plugCategoryIdentifier": "intrinsics" }
              }
            }
            """;

        var rollData = Destiny2WeaponDataProvider.BuildRollData(
            inventoryItemJson,
            "{}",
            "{}",
            "{}",
            "{}",
            BreakerTypeJson);

        rollData.Weapons.Keys.Order().ShouldBe([200u, 201u]);
        rollData.Weapons[200].FrameHash.ShouldBe(10u);
        rollData.Weapons[200].PerkSockets.ShouldBeEmpty();
        rollData.Weapons[200].Breaker.Type.ShouldBe(Destiny2BreakerTypeEnum.ShieldPiercing);
        rollData.Weapons[200].Breaker.Name.ShouldBe("Barrier Champions");
        rollData.Weapons[201].FrameHash.ShouldBe(0u);
        rollData.Weapons[201].PerkSockets.ShouldBeEmpty();
        rollData.Weapons[201].Breaker.Type.ShouldBe(Destiny2BreakerTypeEnum.Disruption);
        rollData.Weapons[201].Breaker.Name.ShouldBe("Overload Champions");

        var weaponTypes = Destiny2WeaponDataProvider.BuildWeaponTypeData(inventoryItemJson);
        var service = new Destiny2WeaponService(
            new FakeWeaponDataProvider(weaponTypes, rollData),
            new Destiny2Setting(new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Destiny2:SyncLanguages:0"] = "en"
                })
                .Build()),
            new FakeBackgroundJobClient());
        var response = await service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                Language = "en",
                BreakerTypes = [Destiny2BreakerTypeEnum.ShieldPiercing]
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Items.Select(weapon => weapon.WeaponHash).ShouldBe([200u]);
        response.Data.Items.Single().MatchedPerkSockets.ShouldBeEmpty();
        response.Data.Items.Single().Breaker.Name.ShouldBe("Barrier Champions");
    }

    [Fact]
    public async Task GetWeaponTypesShouldReturnOnlyTypesUsedByValidWeapons()
    {
        var response = await _service.GetWeaponTypesAsync(
            new GetDestiny2WeaponTypesRequest { Language = "en" },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.WeaponTypes.Select(type => type.WeaponType).ShouldBe([
            Destiny2ItemSubTypeEnum.AutoRifle,
            Destiny2ItemSubTypeEnum.SubmachineGun
        ]);
        response.Data.WeaponTypes.Single(type => type.WeaponType == Destiny2ItemSubTypeEnum.AutoRifle)
            .WeaponCount.ShouldBe(1);
        _dataProvider.RollDataCallCount.ShouldBe(0);
    }

    [Fact]
    public async Task GetPerksShouldSupportMultipleWeaponTypesAndLocalizedKeyword()
    {
        var response = await _service.GetPerksAsync(
            new GetDestiny2WeaponPerksRequest
            {
                WeaponTypes = [
                    Destiny2ItemSubTypeEnum.AutoRifle,
                    Destiny2ItemSubTypeEnum.SubmachineGun,
                    (Destiny2ItemSubTypeEnum)999
                ],
                Language = "en",
                Keyword = "damage"
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.WeaponTypes.Count.ShouldBe(2);
        response.Data.WeaponTypes.Single(type => type.WeaponType.WeaponType == Destiny2ItemSubTypeEnum.AutoRifle)
            .PerkSockets.Single().Perks.Select(perk => perk.PerkHash).ShouldBe([22u]);
        response.Data.WeaponTypes.Single(type => type.WeaponType.WeaponType == Destiny2ItemSubTypeEnum.SubmachineGun)
            .PerkSockets.ShouldBeEmpty();
        response.Data.UnmatchedWeaponTypes.ShouldBe([(Destiny2ItemSubTypeEnum)999]);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public async Task GetPerksShouldReturnAllCurrentPerksWithoutKeyword(string? keyword)
    {
        var response = await _service.GetPerksAsync(
            new GetDestiny2WeaponPerksRequest
            {
                WeaponTypes = [Destiny2ItemSubTypeEnum.AutoRifle],
                Language = "en",
                Keyword = keyword
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.WeaponTypes.Count.ShouldBe(1);
        response.Data.WeaponTypes.Single().PerkSockets
            .SelectMany(socket => socket.Perks)
            .Select(perk => perk.PerkHash)
            .Order()
            .ShouldBe([20u, 22u]);
        var variants = response.Data.WeaponTypes.Single().PerkSockets
            .SelectMany(socket => socket.Perks)
            .Single(perk => perk.PerkHash == 20)
            .Variants;
        variants.Select(variant => variant.PerkHash).ShouldBe([20u, 24u]);
        variants.Select(variant => variant.IsEnhanced).ShouldBe([false, true]);
    }

    [Fact]
    public async Task FilterGodRollShouldSearchAllWeaponTypesWhenTypesAreNotSpecified()
    {
        var response = await _service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                Language = "en",
                PerkGroups =
                [
                    new Destiny2WeaponPerkGroupDto { PerkHashes = [24] }
                ]
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.PageIndex.ShouldBe(1);
        response.Data.PageSize.ShouldBe(20);
        response.Data.Total.ShouldBe(2);
        response.Data.TotalPages.ShouldBe(1);
        response.Data.HasPrevPage.ShouldBeFalse();
        response.Data.HasNextPage.ShouldBeFalse();
        response.Data.Items.Count().ShouldBe(2);
        response.Data.Items.Select(weapon => weapon.Season.SeasonNumber).ShouldBe([24, 23]);
        response.Data.Items.Select(weapon => weapon.WeaponType.WeaponType).ShouldBe([
            Destiny2ItemSubTypeEnum.SubmachineGun,
            Destiny2ItemSubTypeEnum.AutoRifle
        ]);
        var autoRifle = response.Data.Items.ElementAt(1);
        autoRifle.WeaponHash.ShouldBe(100u);
        autoRifle.Frame.FrameHash.ShouldBe(10u);
        autoRifle.Frame.Name.ShouldBe("Adaptive Frame");
        autoRifle.RoundsPerMinute.ShouldBe(600);
        autoRifle.Rarity.ShouldBe(Destiny2TierTypeEnum.Superior);
        autoRifle.Breaker.Type.ShouldBe(Destiny2BreakerTypeEnum.ShieldPiercing);
        autoRifle.Breaker.Hash.ShouldBe(300u);
        autoRifle.Breaker.Icon.ShouldBe("/barrier.png");
        autoRifle.IsCraftable.ShouldBeTrue();
        autoRifle.Stats.Single(stat => stat.StatHash == 12345).Value.ShouldBe(75);
        autoRifle.Foundries.Single().TraitId.ShouldBe("item.weapon.foundry.suros");
        autoRifle.Version.PowerCapHashes.ShouldBe([720u]);
        autoRifle.IsAdept.ShouldBeTrue();
        autoRifle.Appearance.Screenshot.ShouldBe("/screenshot-100.jpg");
        autoRifle.Appearance.IconWatermark.ShouldBe("/watermark-100.png");
        autoRifle.Appearance.BackgroundColor.Red.ShouldBe(10);
        autoRifle.Season.SeasonHash.ShouldBe(900u);
        autoRifle.Season.SeasonNumber.ShouldBe(23);
        autoRifle.Season.Name.ShouldBe("Season of the Wish");
        autoRifle.MatchedPerkSockets.Single().SocketIndex.ShouldBe(1);
        autoRifle.MatchedPerkSockets.Single().Perks.Single().PerkHash.ShouldBe(20u);
        var submachineGun = response.Data.Items.First();
        submachineGun.WeaponHash.ShouldBe(101u);
        submachineGun.MatchedPerkSockets.Single().SocketIndex.ShouldBe(1);
        submachineGun.MatchedPerkSockets.Single().Perks.Single().PerkHash.ShouldBe(24u);
    }

    [Fact]
    public async Task FilterGodRollShouldReturnRequestedPage()
    {
        var response = await _service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                Language = "en",
                PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [24] }],
                PageIndex = 2,
                PageSize = 1
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.PageIndex.ShouldBe(2);
        response.Data.PageSize.ShouldBe(1);
        response.Data.Total.ShouldBe(2);
        response.Data.TotalPages.ShouldBe(2);
        response.Data.HasPrevPage.ShouldBeTrue();
        response.Data.HasNextPage.ShouldBeFalse();
        response.Data.Items.Select(weapon => weapon.WeaponHash).ShouldBe([100u]);
    }

    [Fact]
    public async Task FilterGodRollShouldMatchOrWithinGroupsAndAndAcrossGroups()
    {
        var response = await _service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                WeaponTypes = [Destiny2ItemSubTypeEnum.AutoRifle],
                Language = "en",
                PerkGroups =
                [
                    new Destiny2WeaponPerkGroupDto { PerkHashes = [24] },
                    new Destiny2WeaponPerkGroupDto { PerkHashes = [22] }
                ]
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Items.Count().ShouldBe(1);
        var weapon = response.Data.Items.Single();
        weapon.WeaponHash.ShouldBe(100u);
        weapon.MatchedPerkSockets.Select(socket => socket.SocketIndex).ShouldBe([1, 2]);
        weapon.MatchedPerkSockets.SelectMany(socket => socket.Perks).Select(perk => perk.PerkHash)
            .ShouldBe([20u, 22u]);
        var responseJson = JsonSerializer.Serialize(response).ToLowerInvariant();
        responseJson.ShouldNotContain("groupindex");
        responseJson.ShouldNotContain("groupid");
    }

    [Fact]
    public async Task FilterGodRollShouldApplyAmmoDamageAndSlotFilters()
    {
        var requests = new[]
        {
            new FilterDestiny2WeaponGodRollRequest
            {
                AmmoTypes = [Destiny2AmmunitionTypeEnum.Special]
            },
            new FilterDestiny2WeaponGodRollRequest
            {
                DamageTypes = [Destiny2DamageTypeEnum.Arc]
            },
            new FilterDestiny2WeaponGodRollRequest
            {
                WeaponSlots = [Destiny2WeaponSlotEnum.Energy]
            }
        };

        foreach (var request in requests)
        {
            request.Language = "en";
            request.PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [24] }];

            var response = await _service.FilterGodRollAsync(request, CancellationToken.None);

            response.Data.ShouldNotBeNull();
            response.Data.Items.Select(weapon => weapon.WeaponHash).ShouldBe([101u]);
        }
    }

    [Fact]
    public async Task FilterGodRollShouldApplyExtendedWeaponFilters()
    {
        var requests = new[]
        {
            new FilterDestiny2WeaponGodRollRequest { Rarities = [Destiny2TierTypeEnum.Superior] },
            new FilterDestiny2WeaponGodRollRequest
            {
                BreakerTypes = [Destiny2BreakerTypeEnum.ShieldPiercing]
            },
            new FilterDestiny2WeaponGodRollRequest { IsCraftable = true },
            new FilterDestiny2WeaponGodRollRequest
            {
                StatFilters = [new Destiny2WeaponStatFilterDto { StatHash = 12345, MinimumValue = 75 }]
            },
            new FilterDestiny2WeaponGodRollRequest
            {
                FoundryTraitIds = ["item.weapon.foundry.suros"]
            },
            new FilterDestiny2WeaponGodRollRequest { CurrentVersions = [1] },
            new FilterDestiny2WeaponGodRollRequest { PowerCapHashes = [720] },
            new FilterDestiny2WeaponGodRollRequest { IsAdept = true },
            new FilterDestiny2WeaponGodRollRequest { ClassTypes = [Destiny2ClassEnum.Titan] }
        };

        foreach (var request in requests)
        {
            request.Language = "en";
            request.PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [24] }];

            var response = await _service.FilterGodRollAsync(request, CancellationToken.None);

            response.Data.ShouldNotBeNull();
            response.Data.Items.Select(weapon => weapon.WeaponHash).ShouldBe([100u]);
        }

        var holofoilResponse = await _service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                IsHolofoil = true,
                Language = "en",
                PerkGroups = [new Destiny2WeaponPerkGroupDto { PerkHashes = [24] }]
            },
            CancellationToken.None);
        holofoilResponse.Data.ShouldNotBeNull();
        holofoilResponse.Data.Items.Select(weapon => weapon.WeaponHash).ShouldBe([101u]);
    }

    [Fact]
    public async Task FilterGodRollShouldRejectGroupsThatOnlyShareOneSocket()
    {
        var response = await _service.FilterGodRollAsync(
            new FilterDestiny2WeaponGodRollRequest
            {
                WeaponTypes = [Destiny2ItemSubTypeEnum.AutoRifle],
                Language = "en",
                PerkGroups =
                [
                    new Destiny2WeaponPerkGroupDto { PerkHashes = [20] },
                    new Destiny2WeaponPerkGroupDto { PerkHashes = [20] }
                ]
            },
            CancellationToken.None);

        response.Data.ShouldNotBeNull();
        response.Data.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task RebuildCacheShouldQueueBackgroundJob()
    {
        var response = await _service.RebuildCacheAsync(
            new RebuildDestiny2WeaponCacheCommand { Languages = ["en"] },
            CancellationToken.None);

        response.Code.ShouldBe(HttpStatusCode.Accepted);
        response.Data.ShouldNotBeNull();
        response.Data.JobId.ShouldBe("test-job");
        response.Data.Languages.ShouldBe(["en"]);
    }

    private sealed class FakeWeaponDataProvider(
        IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> weaponTypes,
        Destiny2WeaponRollDataDto rollData) : IDestiny2WeaponDataProvider
    {
        public int RollDataCallCount { get; private set; }

        public Task<IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>?> GetWeaponTypesAsync(
            string language,
            CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto>?>(weaponTypes);
        }

        public Task<Destiny2WeaponRollDataDto?> GetRollDataAsync(
            string language,
            IReadOnlyCollection<Destiny2ItemSubTypeEnum>? weaponTypes,
            CancellationToken cancellationToken)
        {
            RollDataCallCount++;
            return Task.FromResult<Destiny2WeaponRollDataDto?>(rollData);
        }

        public Task<bool> RefreshAsync(string language, CancellationToken cancellationToken)
        {
            return Task.FromResult(true);
        }
    }

    private sealed class FakeBackgroundJobClient : IVoidBotBackgroundJobClient
    {
        public string Enqueue(Expression<Func<Task>> methodCall, string queue = "default") => "test-job";

        public string Enqueue<T>(Expression<Func<T, Task>> methodCall, string queue = "default") => "test-job";

        public string ContinueJobWith(
            string parentJobId,
            Expression<Func<Task>> methodCall,
            string queue = "default") => "test-job";

        public string ContinueJobWith<T>(
            string parentJobId,
            Expression<Func<T, Task>> methodCall,
            string queue = "default") => "test-job";
    }
}
