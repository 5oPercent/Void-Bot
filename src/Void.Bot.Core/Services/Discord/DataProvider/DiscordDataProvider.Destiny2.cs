using Discord;
using SkiaSharp;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Resources.i18n.Destiny2;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2.Manifest;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
    public Task<(List<Embed> Embeds, List<FileAttachment> Attachments)> GenerateDestiny2CareerAsync(
        Destiny2ProfileResponse data, CancellationToken cancellationToken = default);
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
        
        var languageEnum = LanguageExtension.MapDestiny2PlayerLanguageToEnum(_destiny2PlayerData.Language);
        var languageCode = Void.Bot.Message.Extension.LanguageExtension.ToLanguageCode(languageEnum);
        var manifestLanguageCode = languageEnum == Void.Bot.Message.Enum.LanguageEnum.Default
            ? "zhchs"
            : languageCode.Replace("-", string.Empty);
        
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
            
        var embedCareer = embedBuilder
                .WithFooter("By Void Bot")
                .Build();
        
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

}
