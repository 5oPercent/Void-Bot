using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2.Weapon;
using Void.Bot.Message.Constants.Discord;

namespace Void.Bot.Core.Services.Discord.Component.Routes;

public interface IDiscordGodRollPaginationRoute : IDiscordComponentRoute
{
}

public class DiscordGodRollPaginationRoute(
    IServiceScopeFactory scopeFactory,
    ILogger logger) : DiscordComponentRouteBase, IDiscordGodRollPaginationRoute
{
    public const int PageSize = 5;

    private const string CacheKeyPrefix = "discord:d2:gr:";

    protected override string CustomIdPrefix =>
        DiscordConstants.GodRollPaginationCustomIdPrefix;

    public override async Task ExecuteAsync(SocketMessageComponent callback)
    {
        if (!TryParseCustomId(callback.Data.CustomId, out var paginationId, out var pageIndex))
            return;

        try
        {
            using var scope = scopeFactory.CreateScope();
            var cacheManager = scope.ServiceProvider.GetRequiredService<ICacheManager>();
            var state = await cacheManager.Get<DiscordGodRollPaginationState>(
                GetCacheKey(paginationId)).ConfigureAwait(false);
            if (state == null)
            {
                await callback.RespondAsync("分页结果已过期，请重新执行 `/d2 gr`。", ephemeral: true)
                    .ConfigureAwait(false);
                return;
            }

            var pageCount = GetPageCount(state.Data.Items.Count());
            pageIndex = Math.Clamp(pageIndex, 0, pageCount - 1);
            var dataProvider = scope.ServiceProvider.GetRequiredService<IDiscordDataProvider>();
            var result = await dataProvider.GenerateDestiny2GodRollAsync(
                state.Data,
                pageIndex,
                PageSize).ConfigureAwait(false);

            await callback.UpdateAsync(builder =>
            {
                builder.Content = FormatContent(state.Content, pageIndex, pageCount);
                builder.Embeds = result.Embeds.ToArray();
                builder.Components = BuildComponents(paginationId, pageIndex, pageCount);
            }).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.Error(ex,
                "Discord Destiny 2 God Roll pagination failed. CustomId: {CustomId}, UserId: {UserId}",
                callback.Data.CustomId,
                callback.User.Id);

            if (!callback.HasResponded)
            {
                await callback.RespondAsync("翻页失败，请稍后重试。", ephemeral: true).ConfigureAwait(false);
            }
        }
    }

    public static int GetPageCount(int itemCount) => Math.Max(1, (itemCount + PageSize - 1) / PageSize);

    public static string GetCacheKey(string paginationId) => $"{CacheKeyPrefix}{paginationId}";

    public static string FormatContent(string content, int pageIndex, int pageCount) =>
        $"{content} ({pageIndex + 1}/{pageCount})";

    public static MessageComponent BuildComponents(string paginationId, int pageIndex, int pageCount)
    {
        return new ComponentBuilder()
            .WithButton(
                label: "上一页",
                customId: BuildCustomId(paginationId, pageIndex - 1),
                style: ButtonStyle.Secondary,
                disabled: pageIndex == 0)
            .WithButton(
                label: "下一页",
                customId: BuildCustomId(paginationId, pageIndex + 1),
                style: ButtonStyle.Secondary,
                disabled: pageIndex >= pageCount - 1)
            .Build();
    }

    private static string BuildCustomId(string paginationId, int pageIndex) =>
        $"{DiscordConstants.GodRollPaginationCustomIdPrefix}{paginationId}:{pageIndex}";

    private static bool TryParseCustomId(string customId, out string paginationId, out int pageIndex)
    {
        paginationId = string.Empty;
        pageIndex = 0;
        if (!customId.StartsWith(
                DiscordConstants.GodRollPaginationCustomIdPrefix,
                StringComparison.Ordinal))
            return false;

        var values = customId[DiscordConstants.GodRollPaginationCustomIdPrefix.Length..].Split(':', 2);
        if (values.Length != 2 ||
            !Guid.TryParseExact(values[0], "N", out _) ||
            !int.TryParse(values[1], out pageIndex))
        {
            return false;
        }

        paginationId = values[0];
        return true;
    }
}

public class DiscordGodRollPaginationState
{
    public PagedListDto<Destiny2WeaponGodRollDto> Data { get; set; } = new();

    public string Content { get; set; } = string.Empty;
}