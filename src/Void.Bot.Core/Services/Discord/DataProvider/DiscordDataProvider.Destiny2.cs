using Discord;
using SkiaSharp;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.Bungie.Destiny2;
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
        var profileInfo = data.Response.Profiles.Data;
        var commendationInfo = data.Response.ProfileCommendations.Data;

        var globalDisplayName = $"{profileInfo.UserInfo.BungieGlobalDisplayName}#{profileInfo.UserInfo.BungieGlobalDisplayNameCode}";
        var targetPlatform = profileInfo.UserInfo.ApplicableMembershipTypes.Select(x => x.ToEnum<BungieMembershipTypeEnum>().ToString()).ToList();

        #region Embed
        
        var currentSeasonContext = $"""
                                    > 赛季名称: 
                                    > 赛季天数: 
                                    > 光等上限: `{profileInfo.CurrentSeasonRewardPowerCap}`
                                    > 守护者等级: `{profileInfo.RenewedGuardianRank}`
                                    """;
        
        var historyCareerContext = $"""
                                    > 守护者等级: `{profileInfo.LifetimeHighestGuardianRank}`
                                    > 嘉奖得分: `{commendationInfo.TotalScore}`
                                    """;

        var embedCareer = new EmbedBuilder()
            .WithColor(139,139,255)
            .WithTitle($"{globalDisplayName} 的生涯信息")
            .AddField("当前赛季", currentSeasonContext)
            .AddField("历史生涯", historyCareerContext)
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