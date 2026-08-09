using Shouldly;
using System.Text.Json;
using Void.Bot.Core.Services.Bungie.Destiny2.Weapon;
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
            "itemType": 3,
            "itemSubType": 6,
            "itemTypeDisplayName": "Auto Rifle",
            "displayProperties": { "name": "Test Rifle", "description": "A test weapon", "icon": "/weapon-100.png" },
            "stats": {
              "stats": { "4284893193": { "statHash": 4284893193, "value": 600 } }
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
            "itemType": 3,
            "itemSubType": 24,
            "itemTypeDisplayName": "Submachine Gun",
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

    private readonly Destiny2WeaponRollDataDto _rollData;
    private readonly IReadOnlyDictionary<Destiny2ItemSubTypeEnum, Destiny2WeaponTypeDataDto> _weaponTypeData;
    private readonly FakeWeaponDataProvider _dataProvider;
    private readonly Destiny2WeaponService _service;

    public Destiny2WeaponServiceFixture()
    {
        _rollData = Destiny2WeaponDataProvider.BuildRollData(InventoryItemJson, PlugSetJson);
        _weaponTypeData = Destiny2WeaponDataProvider.BuildWeaponTypeData(InventoryItemJson);
        _dataProvider = new FakeWeaponDataProvider(_weaponTypeData, _rollData);
        _service = new Destiny2WeaponService(_dataProvider);
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
        _rollData.Weapons[100].PerkSockets[0].PerkHashes.ShouldContain(20u);
        _rollData.Weapons[100].PerkSockets[0].PerkHashes.ShouldNotContain(21u);
        _rollData.PerkVariantGroups[20].Order().ShouldBe([20u, 24u]);
        _rollData.PerkVariantGroups[24].Order().ShouldBe([20u, 24u]);
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
        response.Data.Weapons.Count.ShouldBe(2);
        response.Data.Weapons.Select(weapon => weapon.WeaponType.WeaponType).ShouldBe([
            Destiny2ItemSubTypeEnum.AutoRifle,
            Destiny2ItemSubTypeEnum.SubmachineGun
        ]);
        response.Data.Weapons[0].WeaponHash.ShouldBe(100u);
        response.Data.Weapons[0].Frame.FrameHash.ShouldBe(10u);
        response.Data.Weapons[0].Frame.Name.ShouldBe("Adaptive Frame");
        response.Data.Weapons[0].RoundsPerMinute.ShouldBe(600);
        response.Data.Weapons[0].MatchedPerkSockets.Single().SocketIndex.ShouldBe(1);
        response.Data.Weapons[0].MatchedPerkSockets.Single().Perks.Single().PerkHash.ShouldBe(20u);
        response.Data.Weapons[1].WeaponHash.ShouldBe(101u);
        response.Data.Weapons[1].MatchedPerkSockets.Single().SocketIndex.ShouldBe(1);
        response.Data.Weapons[1].MatchedPerkSockets.Single().Perks.Single().PerkHash.ShouldBe(24u);
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
        response.Data.Weapons.Count.ShouldBe(1);
        var weapon = response.Data.Weapons.Single();
        weapon.WeaponHash.ShouldBe(100u);
        weapon.MatchedPerkSockets.Select(socket => socket.SocketIndex).ShouldBe([1, 2]);
        weapon.MatchedPerkSockets.SelectMany(socket => socket.Perks).Select(perk => perk.PerkHash)
            .ShouldBe([20u, 22u]);
        var responseJson = JsonSerializer.Serialize(response).ToLowerInvariant();
        responseJson.ShouldNotContain("groupindex");
        responseJson.ShouldNotContain("groupid");
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
        response.Data.Weapons.ShouldBeEmpty();
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

        public Task RefreshAsync(string language, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
