using System.Text.RegularExpressions;
using System.Linq.Expressions;
using Discord;
using Discord.WebSocket;
using Mediator.Net;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n.Destiny2;
using Void.Bot.Core.Resources.i18n.Discord;
using Void.Bot.Core.Services.Discord.Component.Routes;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Commands.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Discord.SlashCommand;

public partial class Destiny2SlashCommand(
    IMediator mediator,
    IDiscordDataProvider dataProvider,
    ILocalizationService localizationService,
    ICacheManager cacheManager) : IGlobalSlashCommand
{
    public bool Ephemeral => false;

    public SlashCommandProperties Command => new SlashCommandBuilder()
        .WithName("d2")
        .WithDescription("提供一系列 Destiny2 相关指令.")
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("生涯")
            .WithDescription("查询玩家生涯信息.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddDestinyPlayerOptions())
        .AddOption(new SlashCommandOptionBuilder()
            .WithName("赛季")
            .WithDescription("查询赛季游玩时长.")
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddDestinyPlayerOptions())
        .AddOption(CreateGodRollSubCommand())
        .Build();
    
    public async Task ExecuteAsync(SocketSlashCommand commandCallback, CancellationToken cancellationToken = default)
    {
        var subCommand = commandCallback.Data.Options.FirstOrDefault();
        if (subCommand == null)
        {
            await ModifyResponseAsync(commandCallback, "未知命令.").ConfigureAwait(false);
            return;
        }

        if (subCommand.Name == "gr")
        {
            await ExecuteGodRollAsync(commandCallback, subCommand, cancellationToken).ConfigureAwait(false);
            return;
        }

        var commandName = subCommand.Name;
        var findBungieName = subCommand.Options.FirstOrDefault(x => x.Name.Equals("BungieName", StringComparison.OrdinalIgnoreCase))?.Value.ToString();
        var findMembership = subCommand.Options.FirstOrDefault(x => x.Name.Equals("Platform", StringComparison.OrdinalIgnoreCase))?.Value.ToEnum<BungieMembershipTypeEnum>();

        if (!string.IsNullOrEmpty(findBungieName) && !BungieIdRegex().IsMatch(findBungieName))
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"请输入正确的 BungieName, 示例: {BungieConstants.StandardGuardianName}.";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var parts = findBungieName?.Split('#', StringSplitOptions.RemoveEmptyEntries);
        
        var profileResponse = await mediator.RequestAsync<GetDestiny2BaseProfileRequest, GetDestiny2BaseProfileResponse>(
            new GetDestiny2BaseProfileRequest
            {
                DisplayName = parts?[0] ?? string.Empty,
                DisplayNameCode = parts?[1] ?? string.Empty,
                MembershipType = findMembership ?? BungieMembershipTypeEnum.None
            }, cancellationToken).ConfigureAwait(false);

        if (profileResponse.Data == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"{profileResponse.Message}";
            }).ConfigureAwait(false);
            
            return;
        }
        
        var displayName = profileResponse.Data.DisplayName;
        var isSinglePlatform = profileResponse.Data.IsSinglePlatform;
        var onCrossSave = profileResponse.Data.OnCrossSave;
        var membershipType = profileResponse.Data.MembershipType;
        var membershipId = profileResponse.Data.MembershipId;
        
        if (!isSinglePlatform && !onCrossSave && findMembership == null)
        {
            await commandCallback.ModifyOriginalResponseAsync(builder =>
            {
                builder.Content = $"查询到未启用跨平台, 需要补充游玩平台参数.";
            }).ConfigureAwait(false);
            return;
        }
        
        switch (commandName)
        {
            case "生涯":
            {
                var response = await mediator.RequestAsync<GetDestiny2CareerRequest, GetDestiny2CareerResponse>(
                    new GetDestiny2CareerRequest
                    {
                        DisplayName = parts?[0] ?? string.Empty,
                        DisplayNameCode = parts?[1] ?? string.Empty,
                        MembershipType = membershipType
                    }, cancellationToken).ConfigureAwait(false);
                
                if (response.Data == null)
                {
                    await commandCallback.ModifyOriginalResponseAsync(builder =>
                    {
                        builder.Content = $"{response.Message}";
                    }).ConfigureAwait(false);
                    
                    return;
                }
                
                var result = await dataProvider.GenerateDestiny2CareerAsync(response.Data, cancellationToken).ConfigureAwait(false);

                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = $"查询到 `{displayName}` 的生涯信息.";
                    builder.Embeds = result.Embeds.ToArray();
                    //builder.Attachments = result.Attachments;
                }).ConfigureAwait(false);
                break;
            }
            case "赛季":
            {
                var response = await mediator.SendAsync<GetDestiny2SeasonCommand, GetDestiny2SeasonResponse>(
                    new GetDestiny2SeasonCommand
                    {
                        DisplayName = parts?[0] ?? string.Empty,
                        DisplayNameCode = parts?[1] ?? string.Empty,
                        MembershipType = membershipType
                    }, cancellationToken).ConfigureAwait(false);

                if (response.Data == null)
                {
                    await commandCallback.ModifyOriginalResponseAsync(builder =>
                    {
                        builder.Content = $"{response.Message}";
                    }).ConfigureAwait(false);
                    
                    return;
                }

                var result = await dataProvider.GenerateDestiny2SeasonAsync(response.Data, cancellationToken).ConfigureAwait(false);

                await commandCallback.ModifyOriginalResponseAsync(builder =>
                {
                    builder.Content = $"查询到 `{displayName}` 的赛季游玩数据.";
                    builder.Embeds = result.Embeds.ToArray();
                }).ConfigureAwait(false);
                break;
            }
        }
    }

    private async Task ExecuteGodRollAsync(
        SocketSlashCommand commandCallback,
        IApplicationCommandInteractionDataOption subCommand,
        CancellationToken cancellationToken)
    {
        var weaponTypeValue = subCommand.Options.FirstOrDefault(option => option.Name == "weapon-type")?.Value;
        var weaponTypes = weaponTypeValue == null
            ? new List<Destiny2ItemSubTypeEnum>()
            : [(Destiny2ItemSubTypeEnum)Convert.ToInt32(weaponTypeValue)];
        var language = commandCallback.UserLocale.ToLowerInvariant() switch
        {
            "zh-cn" => "zh-chs",
            "zh-tw" => "zh-cht",
            "en" => "en",
            var locale when locale.StartsWith("en-", StringComparison.Ordinal) => "en",
            _ => "zh-chs"
        };
        var languageEnum = LanguageExtension.MapI18nCodeToEnum(language);

        var perkInputGroups = subCommand.Options
            .Where(option => option.Name.StartsWith("perks", StringComparison.OrdinalIgnoreCase))
            .OrderBy(option => option.Name.Length)
            .ThenBy(option => option.Name)
            .Select(option => option.Value?.ToString())
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value!
                .Split([',', '，'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .ToList())
            .Where(group => group.Count > 0)
            .ToList();
        if (perkInputGroups.Count == 0)
        {
            await ModifyResponseAsync(commandCallback, localizationService.GetValue<Destiny2GodRollSlashCommandContextTemplate>(context => context.PerkInputRequiredResponse, languageEnum)).ConfigureAwait(false);
            return;
        }

        var perkGroups = new List<Destiny2WeaponPerkGroupDto>();
        var nameInputs = perkInputGroups
            .SelectMany(group => group)
            .Where(input => !uint.TryParse(input, out _))
            .ToList();
        List<Destiny2WeaponPerkDto> availablePerks = [];
        if (nameInputs.Count > 0)
        {
            var perkResponse = await mediator.RequestAsync<GetDestiny2WeaponPerksRequest, GetDestiny2WeaponPerksResponse>(
                new GetDestiny2WeaponPerksRequest
                {
                    WeaponTypes = weaponTypes,
                }, cancellationToken).ConfigureAwait(false);
            if (perkResponse.Data == null)
            {
                await ModifyResponseAsync(commandCallback, perkResponse.Message).ConfigureAwait(false);
                return;
            }

            availablePerks = perkResponse.Data.WeaponTypes
                .SelectMany(type => type.PerkSockets)
                .SelectMany(socket => socket.Perks)
                .GroupBy(perk => perk.PerkHash)
                .Select(group => group.First())
                .ToList();
        }

        foreach (var inputGroup in perkInputGroups)
        {
            var perkHashes = new HashSet<uint>();
            foreach (var input in inputGroup)
            {
                if (uint.TryParse(input, out var perkHash))
                {
                    perkHashes.Add(perkHash);
                    continue;
                }

                var exactMatches = availablePerks
                    .Where(perk => string.Equals(perk.Name, input, StringComparison.OrdinalIgnoreCase))
                    .ToList();
                var matches = exactMatches.Count > 0
                    ? exactMatches
                    : availablePerks
                        .Where(perk => perk.Name.Contains(input, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                if (matches.Count == 0)
                {
                    await ModifyResponseAsync(commandCallback, localizationService.GetValue<Destiny2GodRollSlashCommandContextTemplate>(context => context.PerkNotFoundResponse, languageEnum, input)).ConfigureAwait(false);
                    return;
                }

                if (matches.Count > 1)
                {
                    var candidates = string.Join(", ", matches.Select(perk => $"{perk.Name} ({perk.PerkHash})"));
                    await ModifyResponseAsync(commandCallback, localizationService.GetValue<Destiny2GodRollSlashCommandContextTemplate>(context => context.PerkNameAmbiguousResponse, languageEnum, input, candidates)).ConfigureAwait(false);
                    return;
                }

                perkHashes.Add(matches[0].PerkHash);
            }

            perkGroups.Add(new Destiny2WeaponPerkGroupDto { PerkHashes = perkHashes.ToList() });
        }

        var response = await mediator.RequestAsync<FilterDestiny2WeaponGodRollRequest, FilterDestiny2WeaponGodRollResponse>(
            new FilterDestiny2WeaponGodRollRequest
            {
                WeaponTypes = weaponTypes,
                PerkGroups = perkGroups,
                PageSize = 100
            }, cancellationToken).ConfigureAwait(false);
        if (response.Data == null)
        {
            await ModifyResponseAsync(commandCallback, response.Message).ConfigureAwait(false);
            return;
        }

        if (!response.Data.Items.Any())
        {
            await ModifyResponseAsync(commandCallback, localizationService.GetValue<Destiny2GodRollSlashCommandContextTemplate>(context => context.NoMatchingWeaponsResponse, languageEnum)).ConfigureAwait(false);
            return;
        }

        var result = await dataProvider.GenerateDestiny2GodRollAsync(
            response.Data,
            pageSize: DiscordGodRollPaginationRoute.PageSize,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        var pageCount = DiscordGodRollPaginationRoute.GetPageCount(response.Data.Items.Count());
        var content = localizationService.GetValue<Destiny2GodRollSlashCommandContextTemplate>(context => context.MatchingWeaponsResponse, languageEnum, response.Data.Items.Count());
        string? paginationId = null;
        if (pageCount > 1)
        {
            paginationId = Guid.NewGuid().ToString("N");
            await cacheManager.Set(
                DiscordGodRollPaginationRoute.GetCacheKey(paginationId),
                new DiscordGodRollPaginationState
                {
                    Data = response.Data,
                    Content = content
                },
                TimeSpan.FromMinutes(15),
                cancellationToken).ConfigureAwait(false);
        }

        await commandCallback.ModifyOriginalResponseAsync(builder =>
        {
            builder.Content = DiscordGodRollPaginationRoute.FormatContent(content, 0, pageCount);
            builder.Embeds = result.Embeds.ToArray();
            if (paginationId != null)
            {
                builder.Components = DiscordGodRollPaginationRoute.BuildComponents(paginationId, 0, pageCount);
            }
        }).ConfigureAwait(false);
    }

    private SlashCommandOptionBuilder CreateGodRollSubCommand()
    {
        var commandDescriptions = localizationService.GetValues<Destiny2GodRollSlashCommandContextTemplate>(context => context.CommandDescription);
        var weaponTypeDescriptions = localizationService.GetValues<Destiny2GodRollSlashCommandContextTemplate>(context => context.WeaponTypeOptionDescription);
        var weaponType = new SlashCommandOptionBuilder()
            .WithName("weapon-type")
            .WithDescription(weaponTypeDescriptions["zh-CN"])
            .WithDescriptionLocalizations(weaponTypeDescriptions)
            .WithType(ApplicationCommandOptionType.Integer)
            .WithRequired(true);
        var weaponTypeNames = new Dictionary<Destiny2ItemSubTypeEnum, Expression<Func<Destiny2WeaponContextTemplate, object>>>
        {
            [Destiny2ItemSubTypeEnum.AutoRifle] = context => context.AutoRifle,
            [Destiny2ItemSubTypeEnum.Shotgun] = context => context.Shotgun,
            [Destiny2ItemSubTypeEnum.Machinegun] = context => context.Machinegun,
            [Destiny2ItemSubTypeEnum.HandCannon] = context => context.HandCannon,
            [Destiny2ItemSubTypeEnum.RocketLauncher] = context => context.RocketLauncher,
            [Destiny2ItemSubTypeEnum.FusionRifle] = context => context.FusionRifle,
            [Destiny2ItemSubTypeEnum.SniperRifle] = context => context.SniperRifle,
            [Destiny2ItemSubTypeEnum.PulseRifle] = context => context.PulseRifle,
            [Destiny2ItemSubTypeEnum.ScoutRifle] = context => context.ScoutRifle,
            [Destiny2ItemSubTypeEnum.Sidearm] = context => context.Sidearm,
            [Destiny2ItemSubTypeEnum.Sword] = context => context.Sword,
            [Destiny2ItemSubTypeEnum.FusionRifleLine] = context => context.FusionRifleLine,
            [Destiny2ItemSubTypeEnum.GrenadeLauncher] = context => context.GrenadeLauncher,
            [Destiny2ItemSubTypeEnum.SubmachineGun] = context => context.SubmachineGun,
            [Destiny2ItemSubTypeEnum.TraceRifle] = context => context.TraceRifle,
            [Destiny2ItemSubTypeEnum.Bow] = context => context.Bow,
            [Destiny2ItemSubTypeEnum.Glaive] = context => context.Glaive
        };
        foreach (var choice in weaponTypeNames)
        {
            var localizations = localizationService.GetValues(choice.Value);
            weaponType.AddChoice(localizations["zh-CN"], (int)choice.Key, localizations);
        }
        var firstPerkDescriptions = localizationService.GetValues<Destiny2GodRollSlashCommandContextTemplate>(context => context.PerkOptionDescription, 1);
        var command = new SlashCommandOptionBuilder()
            .WithName("gr")
            .WithDescription(commandDescriptions["zh-CN"])
            .WithDescriptionLocalizations(commandDescriptions)
            .WithType(ApplicationCommandOptionType.SubCommand)
            .AddOption(weaponType)
            .AddOption(new SlashCommandOptionBuilder()
                .WithName("perks1")
                .WithDescription(firstPerkDescriptions["zh-CN"])
                .WithDescriptionLocalizations(firstPerkDescriptions)
                .WithType(ApplicationCommandOptionType.String)
                .WithRequired(true));
        for (var index = 2; index <= 3; index++)
        {
            var descriptions = localizationService.GetValues<Destiny2GodRollSlashCommandContextTemplate>(context => context.PerkOptionDescription, index);
            command.AddOption(new SlashCommandOptionBuilder()
                .WithName($"perks{index}")
                .WithDescription(descriptions["zh-CN"])
                .WithDescriptionLocalizations(descriptions)
                .WithType(ApplicationCommandOptionType.String));
        }

        return command;
    }

    private static async Task ModifyResponseAsync(SocketSlashCommand commandCallback, string message)
    {
        await commandCallback.ModifyOriginalResponseAsync(builder => builder.Content = message).ConfigureAwait(false);
    }

    [GeneratedRegex(BungieConstants.IdPattern)]
    private static partial Regex BungieIdRegex();
}