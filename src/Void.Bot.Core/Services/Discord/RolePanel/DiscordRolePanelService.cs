using System.Net;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Void.Bot.Core.Domain.Discord;
using Void.Bot.Core.Resources.i18n.Discord;
using Void.Bot.Core.Services.Discord.Context;
using Void.Bot.Core.Services.Language;
using Void.Bot.Message.Commands.Discord;
using Void.Bot.Message.Constants.Discord;
using Void.Bot.Message.Dto.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Services.Discord.RolePanel;

public interface IDiscordRolePanelService : IScope
{
    Task<GetDiscordGuildRoleListResponse> GetGuildRoleListAsync(
        GetDiscordGuildRoleListRequest request,
        CancellationToken cancellationToken);

    Task<GetDiscordRolePanelConfigResponse> GetConfigAsync(
        GetDiscordRolePanelConfigRequest request,
        CancellationToken cancellationToken);

    Task<SaveDiscordRolePanelResponse> SaveConfigAsync(
        SaveDiscordRolePanelCommand command,
        CancellationToken cancellationToken);

    Task<DeleteDiscordRolePanelResponse> DeleteConfigAsync(
        DeleteDiscordRolePanelCommand command,
        CancellationToken cancellationToken);

    Task<PublishDiscordRolePanelResponse> PublishAsync(
        PublishDiscordRolePanelCommand command,
        CancellationToken cancellationToken);

    Task<RefreshDiscordRolePanelResponse> RefreshAsync(
        RefreshDiscordRolePanelCommand command,
        CancellationToken cancellationToken);

    Task<UnpublishDiscordRolePanelResponse> UnpublishAsync(
        UnpublishDiscordRolePanelCommand command,
        CancellationToken cancellationToken);
    
    Task HandleButtonAsync(SocketMessageComponent callback, CancellationToken cancellationToken = default);
}

public sealed class DiscordRolePanelService(
    IDiscordRolePanelDataProvider dataProvider,
    DiscordSocketClient client,
    IDiscordGuildContext guildContext,
    ILocalizationService localizationService) : IDiscordRolePanelService
{
    public async Task<GetDiscordGuildRoleListResponse> GetGuildRoleListAsync(
        GetDiscordGuildRoleListRequest request,
        CancellationToken cancellationToken)
    {
        var response = new GetDiscordGuildRoleListResponse();
        var guild = guildContext.Guild;
        var botUser = guildContext.BotMember;
        response.Data = guild.Roles.OrderByDescending(x => x.Position).Select(role =>
        {
            var assignable = !role.IsEveryone && !role.IsManaged && botUser.Hierarchy > role.Position;
            return new DiscordGuildRoleDto
            {
                Id = role.Id.ToString(),
                Name = role.Name,
                Color = role.Color.ToString(),
                Position = role.Position,
                Managed = role.IsManaged,
                Assignable = assignable,
                UnassignableReason = assignable
                    ? null
                    : role.IsEveryone
                        ? localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.EveryoneRoleUnassignableReason)
                        : role.IsManaged
                            ? localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.ManagedRoleUnassignableReason)
                            : localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.HigherRoleUnassignableReason)
            };
        }).ToList();
        return response;
    }

    public async Task<GetDiscordRolePanelConfigResponse> GetConfigAsync(
        GetDiscordRolePanelConfigRequest request,
        CancellationToken cancellationToken)
    {
        var response = new GetDiscordRolePanelConfigResponse();
        response.Data = ToDto(await dataProvider.GetAsync(guildContext.GuildId, cancellationToken).ConfigureAwait(false));
        return response;
    }

    public async Task<SaveDiscordRolePanelResponse> SaveConfigAsync(
        SaveDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        var response = new SaveDiscordRolePanelResponse();
        if (!ulong.TryParse(command.ChannelId, out var channelId))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.ChannelIdInvalid);
            return response;
        }
        var guildId = guildContext.GuildId;
        var guild = guildContext.Guild;
        var channel = guild.GetTextChannel(channelId);
        if (channel == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.TextChannelUnavailable);
            return response;
        }

        if (command.Items.Count > 25 ||
            command.Items.Any(x => string.IsNullOrWhiteSpace(x.Label) || !ulong.TryParse(x.RoleId, out _)))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.ItemsInvalid);
            return response;
        }

        if (command.Items.Select(x => x.RoleId).Distinct(StringComparer.Ordinal).Count() != command.Items.Count ||
            command.Items.Any(x => !IsSupportedStyle(x.Style) || !IsSupportedMode(x.Mode)))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.InvalidButtonSettings);
            return response;
        }
        var roles = guild.Roles.ToDictionary(x => x.Id);
        if (command.Items.Any(x => !roles.TryGetValue(ulong.Parse(x.RoleId), out var role) || role.IsEveryone || role.IsManaged || guildContext.BotMember.Hierarchy <= role.Position))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.UnassignableRoles);
            return response;
        }
        var panel = await dataProvider.GetAsync(guildId, cancellationToken).ConfigureAwait(false) ?? new DiscordRolePanel { GuildId = guildId };
        panel.ChannelId = channelId;
        panel.Content = command.Content ?? string.Empty;
        panel.Enabled = command.Enabled;
        panel.Items.Clear();
        panel.Items.AddRange(command.Items.OrderBy(x => x.Sort).Select(x => new DiscordRolePanelItem
        {
            Id = Guid.Empty,
            PanelId = panel.Id,
            RoleId = ulong.Parse(x.RoleId),
            Label = x.Label.Trim(),
            Style = x.Style,
            Mode = x.Mode,
            Sort = x.Sort,
            Enabled = x.Enabled
        }));

        if (panel.CreatedAt == default)
        {
            await dataProvider.CreateAsync(panel, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            await dataProvider.UpdateAsync(panel, cancellationToken).ConfigureAwait(false);
        }
        response.Data = ToDto(panel);
        return response;
    }

    public async Task<DeleteDiscordRolePanelResponse> DeleteConfigAsync(DeleteDiscordRolePanelCommand command, CancellationToken cancellationToken)
    {
        var response = new DeleteDiscordRolePanelResponse();
        var panel = await dataProvider.GetAsync(guildContext.GuildId, cancellationToken).ConfigureAwait(false);
        if (panel == null)
        {
            return response;
        }

        await dataProvider.DeleteAsync(panel, cancellationToken).ConfigureAwait(false);
        return response;
    }

    public Task<PublishDiscordRolePanelResponse> PublishAsync(
        PublishDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        return PublishCoreAsync(cancellationToken);
    }

    public Task<RefreshDiscordRolePanelResponse> RefreshAsync(
        RefreshDiscordRolePanelCommand command,
        CancellationToken cancellationToken)
    {
        return RefreshCoreAsync(cancellationToken);
    }

    public async Task<UnpublishDiscordRolePanelResponse> UnpublishAsync(UnpublishDiscordRolePanelCommand command, CancellationToken cancellationToken)
    {
        var response = new UnpublishDiscordRolePanelResponse();
        var panel = await dataProvider.GetAsync(guildContext.GuildId, cancellationToken).ConfigureAwait(false);
        if (panel?.MessageId is not { } messageId)
        {
            return response;
        }

        if (guildContext.Guild.GetTextChannel(panel.ChannelId) is { } channel)
        {
            try
            {
                await channel.DeleteMessageAsync(messageId);
            }
            catch (HttpException)
            {
            }
        }

        panel.MessageId = null;
        await dataProvider.UpdateAsync(panel, cancellationToken);
        return response;
    }

    public async Task HandleButtonAsync(SocketMessageComponent callback, CancellationToken cancellationToken = default)
    {
        var parts = callback.Data.CustomId.Split(':');
        if (parts.Length != 3 || !Guid.TryParse(parts[1], out var panelId) || !Guid.TryParse(parts[2], out var itemId))
        {
            return;
        }

        var panel = await dataProvider.GetAsync(callback.GuildId ?? 0, cancellationToken).ConfigureAwait(false);
        var item = panel?.Id == panelId
            ? panel.Items.FirstOrDefault(x => x.Id == itemId && x.Enabled)
            : null;
        if (item == null || !panel!.Enabled || panel.MessageId != callback.Message.Id || panel.ChannelId != callback.Channel.Id)
        {
            await callback.RespondAsync(localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.PanelExpired, callback.UserLocale), ephemeral: true);
            return;
        }

        var user = callback.User as SocketGuildUser;
        var role = client.GetGuild(callback.GuildId ?? 0)?.GetRole(item.RoleId);
        if (user == null || role == null)
        {
            return;
        }

        var hasRole = user.Roles.Any(x => x.Id == role.Id);
        if (item.Mode.Equals("AddOnly", StringComparison.OrdinalIgnoreCase) && hasRole)
        {
            await callback.RespondAsync(localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.AlreadyHasRole, callback.UserLocale), ephemeral: true);
            return;
        }

        if (hasRole)
        {
            await user.RemoveRoleAsync(role);
        }
        else
        {
            await user.AddRoleAsync(role);
        }
        await callback.RespondAsync(
            hasRole
                ? localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.RoleRemoved, callback.UserLocale, role.Name)
                : localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.RoleAdded, callback.UserLocale, role.Name),
            ephemeral: true);
    }

    private async Task<PublishDiscordRolePanelResponse> PublishCoreAsync(CancellationToken cancellationToken)
    {
        var response = new PublishDiscordRolePanelResponse();
        var panel = await dataProvider.GetAsync(guildContext.GuildId, cancellationToken).ConfigureAwait(false);
        var channel = panel == null ? null : guildContext.Guild.GetTextChannel(panel.ChannelId);
        if (panel == null || channel == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.PanelOrChannelNotFound);
            return response;
        }

        if (panel.MessageId.HasValue)
        {
            response.Code = HttpStatusCode.Conflict;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.PanelAlreadyPublished);
            return response;
        }

        var message = await channel.SendMessageAsync(
            panel.Content,
            components: BuildComponents(panel));
        panel.MessageId = message.Id;
        await dataProvider.UpdateAsync(panel, cancellationToken);
        response.Data = ToPublishDto(panel, true);
        return response;
    }

    private async Task<RefreshDiscordRolePanelResponse> RefreshCoreAsync(CancellationToken cancellationToken)
    {
        var response = new RefreshDiscordRolePanelResponse();
        var panel = await dataProvider.GetAsync(guildContext.GuildId, cancellationToken).ConfigureAwait(false);
        var channel = panel == null ? null : guildContext.Guild.GetTextChannel(panel.ChannelId);
        if (panel?.MessageId is not { } messageId || channel == null)
        {
            response.Code = HttpStatusCode.NotFound;
            response.Message = localizationService.GetValue<DiscordRolePanelContextTemplate>(x => x.PublishedPanelNotFound);
            return response;
        }

        try
        {
            await channel.ModifyMessageAsync(messageId, properties =>
            {
                properties.Content = panel.Content;
                properties.Components = BuildComponents(panel);
            });
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.UnknownMessage)
        {
            panel.MessageId = null;
            var message = await channel.SendMessageAsync(
                panel.Content,
                components: BuildComponents(panel));
            panel.MessageId = message.Id;
            await dataProvider.UpdateAsync(panel, cancellationToken).ConfigureAwait(false);
            response.Data = ToPublishDto(panel, true);
            return response;
        }

        response.Data = ToPublishDto(panel, false);
        return response;
    }

    private static MessageComponent BuildComponents(DiscordRolePanel panel)
    {
        var builder = new ComponentBuilder();
        foreach (var item in panel.Items.Where(x => x.Enabled).OrderBy(x => x.Sort))
        {
            builder.WithButton(
                item.Label,
                $"{DiscordConstants.RolePanelButtonCustomIdPrefix}{panel.Id}:{item.Id}",
                ParseStyle(item.Style));
        }

        return builder.Build();
    }

    private static ButtonStyle ParseStyle(string style)
    {
        return Enum.TryParse<ButtonStyle>(style, true, out var result)
            ? result
            : ButtonStyle.Secondary;
    }

    private static bool IsSupportedStyle(string style)
    {
        return Enum.TryParse<ButtonStyle>(style, true, out var buttonStyle) &&
            buttonStyle is ButtonStyle.Primary or ButtonStyle.Secondary or ButtonStyle.Success or ButtonStyle.Danger;
    }

    private static bool IsSupportedMode(string mode)
    {
        return mode.Equals("Toggle", StringComparison.OrdinalIgnoreCase) ||
            mode.Equals("AddOnly", StringComparison.OrdinalIgnoreCase);
    }

    private static DiscordRolePanelPublishDto ToPublishDto(DiscordRolePanel panel, bool created)
    {
        return new DiscordRolePanelPublishDto
        {
            GuildId = panel.GuildId.ToString(),
            ChannelId = panel.ChannelId.ToString(),
            MessageId = panel.MessageId?.ToString() ?? string.Empty,
            Created = created
        };
    }

    private static DiscordRolePanelDto? ToDto(DiscordRolePanel? panel)
    {
        if (panel == null)
        {
            return null;
        }

        return new DiscordRolePanelDto
        {
            Id = panel.Id,
            GuildId = panel.GuildId.ToString(),
            ChannelId = panel.ChannelId.ToString(),
            MessageId = panel.MessageId?.ToString(),
            Content = panel.Content,
            Enabled = panel.Enabled,
            Items = panel.Items.OrderBy(x => x.Sort).Select(x => new DiscordRolePanelItemDto
            {
                Id = x.Id,
                Label = x.Label,
                RoleId = x.RoleId.ToString(),
                Style = x.Style,
                Mode = x.Mode,
                Sort = x.Sort,
                Enabled = x.Enabled
            }).ToList()
        };
    }
}
