using Discord;
using SkiaSharp;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public partial interface IDiscordDataProvider : IScope
{
    public Task<Embed?> Destiny2CareerBuildEmbedAsync(GetDestinyCareerRequest request, CancellationToken cancellationToken = default);
    
    public Task<Stream?> Destiny2CareerGenerateImageAsync(GetDestinyCareerRequest request, CancellationToken cancellationToken = default);
}

public partial class DiscordDataProvider : IDiscordDataProvider
{
    public async Task<Embed?> Destiny2CareerBuildEmbedAsync(GetDestinyCareerRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.RequestAsync<GetDestinyCareerRequest, GetDestinyCareerResponse>(request, cancellationToken).ConfigureAwait(false);
        if (result.Data == null)
        {
            return null;
        }

        var profileInfo = result.Data.Response.Profile.Data;
        var commendationInfo = result.Data.Response.ProfileCommendations.Data;

        var globalDisplayName = $"{profileInfo.UserInfo.BungieGlobalDisplayName}#{profileInfo.UserInfo.BungieGlobalDisplayNameCode}";
        var targetPlatform = profileInfo.UserInfo.ApplicableMembershipTypes.Select(x => x.ToEnum<BungieMembershipType>().ToString()).ToList();
        
        var embed = new EmbedBuilder()
            .WithColor(139,139,255)
            .WithTitle($"{globalDisplayName} 的生涯信息")
            .AddField("当前赛季:", ">>>>>>>", true)
                .AddField("守护者等级", $"`{profileInfo.CurrentGuardianRank}`", true)
                .AddField("硬光上限", $"`{profileInfo.CurrentSeasonRewardPowerCap}`", true)
            .AddField("历史生涯:", ">>>>>>>", true)
                .AddField("最高守护者等级", $"`{profileInfo.LifetimeHighestGuardianRank}`", true)
                .AddField("嘉奖点赞", $"`{commendationInfo.TotalScore}`", true)
            .AddField("其它数据", ">>>>>>>", true)
                .AddField("角色数量", $"`{profileInfo.CharacterIds.Count}`", true)
                .AddField("遊玩平台", $"`{string.Join(",", targetPlatform)}`", true)
            .WithFooter("By Void Bot")
            .Build();
        
        return embed;
    }

    public async Task<Stream?> Destiny2CareerGenerateImageAsync(GetDestinyCareerRequest request, CancellationToken cancellationToken = default)
    {
        var result = await _mediator.RequestAsync<GetDestinyCareerRequest, GetDestinyCareerResponse>(request, cancellationToken).ConfigureAwait(false);
        if (result.Data == null)
        {
            return null;
        }
        
        var profileInfo = result.Data.Response.Profile.Data;
        var commendationInfo = result.Data.Response.ProfileCommendations.Data;

        var globalDisplayName = $"{profileInfo.UserInfo.BungieGlobalDisplayName}#{profileInfo.UserInfo.BungieGlobalDisplayNameCode}";
        var targetPlatform = profileInfo.UserInfo.ApplicableMembershipTypes.Select(x => x.ToEnum<BungieMembershipType>().ToString()).ToList();
        
        // 绘制图片
        const int width = 280;
        const int height = 220;
        const int textPadding = 8;

        using var surface = SkiaSharpExtension.CreateSurface(width, height);
        var canvas = surface.Canvas;
        
        #region 绘制名称

        // 设置字体
        using var font = new SKFont(SKTypeface.Default, 16);
        font.Edging = SKFontEdging.Antialias;
        font.Typeface = SKTypeface.FromFamilyName("PingFang SC");
        font.GetFontMetrics(out var metrics);
        
        // 绘制文本背景矩形
        using var textBackgroundPaint = new SKPaint();
        textBackgroundPaint.Color = new SKColor(123, 104, 238, 255);
        textBackgroundPaint.IsAntialias = true;
        
        // 计算文本长度
        font.MeasureText(globalDisplayName, out var bounds);
        var textWidth = bounds.Width + textPadding * 3;
        var textHeight = bounds.Height + textPadding * 2;

        var textRect = new SKRect(textPadding, textPadding, textWidth, textHeight);
        var textPath = new SKPath();
        textPath.AddRoundRect(textRect, 10, 10);
        
        canvas.DrawPath(textPath, textBackgroundPaint);
        
        // 绘制文本
        var textX = textRect.Left + textPadding;
        var textY = textRect.Top - metrics.Ascent + textPadding;
        
        using var textPaint = new SKPaint();
        textPaint.Color = SKColors.White;
        textPaint.IsAntialias = true;
        
        canvas.DrawText(globalDisplayName, textX, textY, SKTextAlign.Left, font, textPaint);
        
        #endregion
        
        return surface.ToStream();
    }
}