using Discord;
using SkiaSharp;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n.Destiny2;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
    public Task<(List<Embed> Embeds, List<FileAttachment> Attachments)> GenerateDestiny2CareerAsync(
        Destiny2ProfileResponse data, CancellationToken cancellationToken = default);

    public Task<(List<Embed> Embeds, List<FileAttachment> Attachments)> GenerateDestiny2SeasonAsync(
        Destiny2SeasonStatsDto data, CancellationToken cancellationToken = default);
}

public partial class DiscordDataProvider : IDiscordDataProvider
{
    public async Task<(List<Embed> Embeds, List<FileAttachment> Attachments)> GenerateDestiny2CareerAsync(
        Destiny2ProfileResponse data, CancellationToken cancellationToken = default)
    {
        var profileInfo = data.Response.Profile.Data;
        var commendationInfo = data.Response.ProfileCommendations.Data;

        var globalDisplayName = $"{profileInfo.UserInfo.BungieGlobalDisplayName}#{profileInfo.UserInfo.BungieGlobalDisplayNameCode}";
        var targetPlatform = profileInfo.UserInfo.ApplicableMembershipTypes.Select(x => x.ToEnum<BungieMembershipTypeEnum>().ToString()).ToList();
        
        var languageEnum = LanguageExtension.MapI18nCodeToEnum(_destiny2PlayerData.Language);
        var manifestLanguageCode = languageEnum.ToI18nCode();
        
        #region Embed
        
        var seasonJson = await _cacheManager.Get<string>($"{Destiny2ManifestConstants.SeasonCacheKey}:{manifestLanguageCode}", cancellationToken);
        var seasonDict = seasonJson?.DeserializeJson<Destiny2ManifestSeasonDto>();
        var currentSeason = seasonDict?[profileInfo.CurrentSeasonHash.ToString()];
        
        var seasonTimeRange = $"{currentSeason?.StartDate.ToLocalTime()} - {currentSeason?.EndDate.ToLocalTime()}";
        var seasonDays = $"{(int)(currentSeason?.EndDate.Subtract(currentSeason.StartDate).TotalDays ?? 0)}";
        var seasonRemainingDays = $"{(int)(currentSeason?.EndDate.Subtract(DateTime.UtcNow).TotalDays ?? 0)}";

        var seasonContext = string.Format(
            _languageService.GetContext<Destiny2ContextTemplate>(x => x.SeasonBody, languageEnum),
            currentSeason?.DisplayProperties.Name ?? "???",
            currentSeason != null ? seasonTimeRange : "???",
            seasonRemainingDays, seasonDays,
            profileInfo.CurrentSeasonRewardPowerCap);
        
        var careerContext = string.Format(
            _languageService.GetContext<Destiny2ContextTemplate>(x => x.CareerBody, languageEnum),
            commendationInfo.TotalScore,
            profileInfo.LifetimeHighestGuardianRank,
            profileInfo.RenewedGuardianRank,
            profileInfo.DateLastPlayed.ToLocalTime());

        var careerTitle = string.Format(
            _languageService.GetContext<Destiny2ContextTemplate>(x => x.CareerTitle, languageEnum),
            globalDisplayName);
        
        var careerField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.CareerField, languageEnum);
        var seasonField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.SeasonField, languageEnum);
        
        var embedBuilder = new EmbedBuilder()
            .WithColor(139, 139, 255)
            .WithTitle(careerTitle)
            .AddField(careerField, seasonContext)
            .AddField(seasonField, careerContext);

        if (currentSeason != null)
        {
            embedBuilder.WithThumbnailUrl($"{BungieConstants.BungieUrl}{currentSeason.DisplayProperties.Icon}");
        }
        
        var embedCareer = embedBuilder.BuildWithFooter();
        
        #endregion
        
        #region Image
        
        // 绘制图片
        const int width = 280;
        const int height = 220;
        const int textPadding = 8;

        using var surface = SkiaSharpExtension.CreateSurface(width, height);
        var canvas = surface.Canvas;
        
        // 设置字体
        using var font = new SKFont
        {
            Typeface = SKTypeface.FromStream(FontExtension.SourceHanSansSCFontStream),
            Size = 16,
            Edging = SKFontEdging.Antialias
        };
        font.GetFontMetrics(out var metrics);
        
        // 计算文本长度
        font.MeasureText(globalDisplayName, out var bounds);
        var textWidth = bounds.Width + textPadding * 3;
        var textHeight = bounds.Height + textPadding * 2;

        // 绘制文本背景矩形
        using var textBackgroundPaint = new SKPaint();
        textBackgroundPaint.Color = new SKColor(123, 104, 238, 255);
        textBackgroundPaint.IsAntialias = true;

        var textPath = new SKPath();
        var textRect = new SKRect(textPadding, textPadding, textWidth, textHeight);
        textPath.AddRoundRect(textRect, 10, 10);
        
        canvas.DrawPath(textPath, textBackgroundPaint);
        
        // 绘制文本
        var textX = textRect.Left + textPadding;
        var textY = textRect.Top - metrics.Ascent + textPadding / 2;
        
        using var textPaint = new SKPaint();
        textPaint.Color = SKColors.White;
        textPaint.IsAntialias = true;
        
        canvas.DrawText(globalDisplayName, textX, textY, SKTextAlign.Left, font, textPaint);
        
        #endregion
        
        return ([embedCareer], [new FileAttachment(surface.ToStream(), "destiny2_career.png")]);
    }

    public Task<(List<Embed> Embeds, List<FileAttachment> Attachments)> GenerateDestiny2SeasonAsync(
        Destiny2SeasonStatsDto data, CancellationToken cancellationToken = default)
    {
        var languageEnum = LanguageExtension.MapI18nCodeToEnum(_destiny2PlayerData.Language);
        var seasonTitleTemplate = _languageService.GetContext<Destiny2ContextTemplate>(x => x.SeasonLatestTitle, languageEnum);
        var seasonTitle = string.Format(seasonTitleTemplate, "Destiny 2");
        
        if (!data.HasData)
        {
            var noDataText = _languageService.GetContext<Destiny2ContextTemplate>(x => x.SeasonNoData, languageEnum);

            var noDataEmbed = new EmbedBuilder()
                .WithColor(139, 139, 255)
                .WithTitle(seasonTitle)
                .WithDescription(noDataText)
                .BuildWithFooter();
            return Task.FromResult<(List<Embed> Embeds, List<FileAttachment> Attachments)>(([noDataEmbed], new List<FileAttachment>()));
        }

        var totalMinutes = data.TotalPlaytimeSeconds / 60;
        var totalValue = totalMinutes >= 60
            ? $"{totalMinutes / 60}h {totalMinutes % 60}m"
            : $"{totalMinutes}m";

        static string FormatPlaytime(long seconds)
        {
            var minutes = seconds / 60;
            return minutes >= 60
                ? $"{minutes / 60}h {minutes % 60}m"
                : $"{minutes}m";
        }

        var hunterValue = data.Characters.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Hunter) is { } hunter
            ? FormatPlaytime(hunter.PlaytimeSeconds)
            : "N/A";

        var titanValue = data.Characters.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Titan) is { } titan
            ? FormatPlaytime(titan.PlaytimeSeconds)
            : "N/A";

        var warlockValue = data.Characters.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Warlock) is { } warlock
            ? FormatPlaytime(warlock.PlaytimeSeconds)
            : "N/A";

        var totalField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.SeasonLatestTotalField, languageEnum);
        var hunterField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.HunterField, languageEnum);
        var titanField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.TitanField, languageEnum);
        var warlockField = _languageService.GetContext<Destiny2ContextTemplate>(x => x.WarlockField, languageEnum);

        var embedBuilder = new EmbedBuilder()
            .WithColor(139, 139, 255)
            .WithTitle(seasonTitle)
            .AddField(totalField, totalValue)
            .AddField(hunterField, hunterValue)
            .AddField(titanField, titanValue)
            .AddField(warlockField, warlockValue);

        var embedSeason = embedBuilder.BuildWithFooter();

        const int seasonsPerEmbed = 20;
        var playedSeasons = data.Seasons.Where(x => x.TotalPlaytimeSeconds > 0).ToList();
        var historyEmbeds = playedSeasons
            .Chunk(seasonsPerEmbed)
            .Select((seasonChunk, index) =>
            {
                var historyBuilder = new EmbedBuilder()
                    .WithColor(139, 139, 255)
                    .WithTitle($"{seasonTitle} ({index + 1}/{(int)Math.Ceiling((double)playedSeasons.Count / seasonsPerEmbed)})");

                foreach (var season in seasonChunk)
                {
                    var hunterSeconds = season.Classes.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Hunter)?.PlaytimeSeconds ?? 0;
                    var titanSeconds = season.Classes.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Titan)?.PlaytimeSeconds ?? 0;
                    var warlockSeconds = season.Classes.FirstOrDefault(x => x.Class == Destiny2ClassEnum.Warlock)?.PlaytimeSeconds ?? 0;
                    var seasonName = string.IsNullOrWhiteSpace(season.Name) ? $"Season {season.SeasonNumber}" : season.Name;
                    var seasonValue = $"{totalField}: `{FormatPlaytime(season.TotalPlaytimeSeconds)}`\n" +
                                      $"{hunterField}: `{FormatPlaytime(hunterSeconds)}` · " +
                                      $"{titanField}: `{FormatPlaytime(titanSeconds)}` · " +
                                      $"{warlockField}: `{FormatPlaytime(warlockSeconds)}`";

                    historyBuilder.AddField($"S{season.SeasonNumber} · {seasonName}", seasonValue);
                }

                return historyBuilder.BuildWithFooter();
            })
            .ToList();

        return Task.FromResult<(List<Embed> Embeds, List<FileAttachment> Attachments)>(
            ([embedSeason, .. historyEmbeds], new List<FileAttachment>()));
    }

}