using System.Collections.Concurrent;
using System.Net;
using AutoMapper;
using Discord;
using Discord.WebSocket;
using Serilog;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Discord;
using Void.Bot.Core.Services.Discord.DataProvider;
using Void.Bot.Message.Commands.Discord;
using Void.Bot.Message.Dto.Discord;
using Void.Bot.Message.Request.Discord;

namespace Void.Bot.Core.Services.Discord;

public interface IDiscordVoiceChannelService : IScope
{
    Task<GetDiscordVoiceChannelConfigResponse> GetConfigAsync(GetDiscordVoiceChannelConfigRequest request, CancellationToken cancellationToken);

    Task<GetDiscordVoiceChannelConfigListResponse> GetConfigListAsync(GetDiscordVoiceChannelConfigListRequest request, CancellationToken cancellationToken);

    Task<AddDiscordVoiceChannelConfigResponse> AddConfigAsync(AddDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken);

    Task<UpdateDiscordVoiceChannelConfigResponse> UpdateConfigAsync(UpdateDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken);

    Task<DeleteDiscordVoiceChannelConfigResponse> DeleteConfigAsync(DeleteDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken);

    Task<GetDiscordTemporaryVoiceChannelListResponse> GetTemporaryChannelListAsync(GetDiscordTemporaryVoiceChannelListRequest request, CancellationToken cancellationToken);

    Task HandleVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> SetLimitAsync(SocketGuildUser user, int userLimit, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> SetPrivacyAsync(SocketGuildUser user, bool isPrivate, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> AllowUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> DenyUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> KickUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> DeleteOwnChannelAsync(SocketGuildUser user, CancellationToken cancellationToken = default);

    Task<DiscordVoiceCommandResult> GetInfoAsync(SocketGuildUser user, CancellationToken cancellationToken = default);

    Task CleanupExpiredEmptyChannelsAsync(CancellationToken cancellationToken = default);
}

public class DiscordVoiceCommandResult
{
    public bool Success { get; set; }

    public string Message { get; set; } = string.Empty;
}

public class DiscordVoiceChannelService : IDiscordVoiceChannelService
{
    private const int MinUserLimit = 3;
    private const int MaxUserLimit = 50;
    private static readonly TimeSpan EmptyChannelLifetime = TimeSpan.FromMinutes(10);
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> UserChannelLocks = new();

    private readonly IMapper _mapper;
    private readonly ILogger _logger;
    private readonly IUnitOfWork _unitOfWork;
    private readonly DiscordSocketClient _client;
    private readonly IDiscordVoiceChannelDataProvider _dataProvider;

    public DiscordVoiceChannelService(
        IMapper mapper,
        ILogger logger,
        IUnitOfWork unitOfWork,
        DiscordSocketClient client,
        IDiscordVoiceChannelDataProvider dataProvider)
    {
        _mapper = mapper;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _client = client;
        _dataProvider = dataProvider;
    }

    public async Task<GetDiscordVoiceChannelConfigResponse> GetConfigAsync(GetDiscordVoiceChannelConfigRequest request, CancellationToken cancellationToken)
    {
        var config = await _dataProvider.GetConfigAsync(request.Id, cancellationToken).ConfigureAwait(false);

        return new GetDiscordVoiceChannelConfigResponse
        {
            Data = _mapper.Map<DiscordVoiceChannelConfigDto>(config)
        };
    }

    public async Task<GetDiscordVoiceChannelConfigListResponse> GetConfigListAsync(GetDiscordVoiceChannelConfigListRequest request, CancellationToken cancellationToken)
    {
        var configs = await _dataProvider.GetConfigsAsync(request.GuildId, cancellationToken).ConfigureAwait(false);

        return new GetDiscordVoiceChannelConfigListResponse
        {
            Data = _mapper.Map<List<DiscordVoiceChannelConfigDto>>(configs)
        };
    }

    public async Task<AddDiscordVoiceChannelConfigResponse> AddConfigAsync(AddDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken)
    {
        var response = new AddDiscordVoiceChannelConfigResponse();
        var validationMessage = await ValidateConfigAsync(command.GuildId, command.TriggerChannelId, command.CreateCategoryId, command.DefaultUserLimit, null, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        var config = _mapper.Map<DiscordVoiceChannelConfig>(command);
        config.ChannelNameTemplate = NormalizeTemplate(config.ChannelNameTemplate);

        await _dataProvider.CreateConfigAsync(config, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<UpdateDiscordVoiceChannelConfigResponse> UpdateConfigAsync(UpdateDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken)
    {
        var response = new UpdateDiscordVoiceChannelConfigResponse();

        var config = await _dataProvider.GetConfigAsync(command.Id, cancellationToken).ConfigureAwait(false);
        if (config == null)
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = "Voice channel config not found.";
            return response;
        }

        var validationMessage = await ValidateConfigAsync(command.GuildId, command.TriggerChannelId, command.CreateCategoryId, command.DefaultUserLimit, command.Id, cancellationToken).ConfigureAwait(false);
        if (!string.IsNullOrEmpty(validationMessage))
        {
            response.Code = HttpStatusCode.BadRequest;
            response.Message = validationMessage;
            return response;
        }

        _mapper.Map(command, config);
        config.ChannelNameTemplate = NormalizeTemplate(config.ChannelNameTemplate);

        await _dataProvider.UpdateConfigAsync(config, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<DeleteDiscordVoiceChannelConfigResponse> DeleteConfigAsync(DeleteDiscordVoiceChannelConfigCommand command, CancellationToken cancellationToken)
    {
        var response = new DeleteDiscordVoiceChannelConfigResponse();

        await _dataProvider.DeleteConfigsAsync(command.IdList, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return response;
    }

    public async Task<GetDiscordTemporaryVoiceChannelListResponse> GetTemporaryChannelListAsync(GetDiscordTemporaryVoiceChannelListRequest request, CancellationToken cancellationToken)
    {
        var channels = await _dataProvider.GetTemporaryChannelsAsync(request.GuildId, request.IncludeDeleted, cancellationToken).ConfigureAwait(false);
        var dtos = _mapper.Map<List<DiscordTemporaryVoiceChannelDto>>(channels);

        if (channels.Count != 0)
        {
            var allowedUsers = await _dataProvider.GetAllowedUsersAsync(channels.Select(x => x.Id).ToList(), cancellationToken).ConfigureAwait(false);
            var allowedUserLookup = allowedUsers
                .GroupBy(x => x.TemporaryVoiceChannelId)
                .ToDictionary(x => x.Key, x => x.Select(y => y.UserId).ToList());

            foreach (var dto in dtos)
            {
                dto.AllowedUserIds = allowedUserLookup.GetValueOrDefault(dto.Id, []);
            }
        }

        return new GetDiscordTemporaryVoiceChannelListResponse
        {
            Data = dtos
        };
    }

    public async Task HandleVoiceStateUpdatedAsync(SocketUser user, SocketVoiceState before, SocketVoiceState after, CancellationToken cancellationToken = default)
    {
        if (user.IsBot || user is not SocketGuildUser guildUser)
        {
            return;
        }

        await RefreshTemporaryChannelEmptyStateAsync(before.VoiceChannel, cancellationToken).ConfigureAwait(false);
        await RefreshTemporaryChannelEmptyStateAsync(after.VoiceChannel, cancellationToken).ConfigureAwait(false);

        if (after.VoiceChannel is not SocketVoiceChannel joinedChannel)
        {
            return;
        }

        var config = await _dataProvider.GetConfigAsync(joinedChannel.Guild.Id, joinedChannel.Id, cancellationToken).ConfigureAwait(false);
        if (config == null)
        {
            return;
        }

        var lockKey = $"{joinedChannel.Guild.Id}:{guildUser.Id}";
        var semaphore = UserChannelLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await MoveUserToOwnedOrNewChannelAsync(guildUser, config, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            semaphore.Release();
        }
    }

    public async Task<DiscordVoiceCommandResult> SetLimitAsync(SocketGuildUser user, int userLimit, CancellationToken cancellationToken = default)
    {
        if (userLimit is < MinUserLimit or > MaxUserLimit)
        {
            return Fail($"人数范围必须在 {MinUserLimit}-{MaxUserLimit} 之间.");
        }

        var (temporaryChannel, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null || channel == null)
        {
            return Fail(error);
        }

        await channel.ModifyAsync(properties => properties.UserLimit = userLimit).ConfigureAwait(false);
        temporaryChannel.UserLimit = userLimit;

        await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok($"频道人数上限已设置为 {userLimit}.");
    }

    public async Task<DiscordVoiceCommandResult> SetPrivacyAsync(SocketGuildUser user, bool isPrivate, CancellationToken cancellationToken = default)
    {
        var (temporaryChannel, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null || channel == null)
        {
            return Fail(error);
        }

        temporaryChannel.IsPrivate = isPrivate;
        await ApplyPrivacyOverwriteAsync(channel, temporaryChannel, cancellationToken).ConfigureAwait(false);

        await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok(isPrivate ? "频道已设置为私密，未允许用户已被踢出." : "频道已设置为公开.");
    }

    public async Task<DiscordVoiceCommandResult> AllowUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default)
    {
        var (temporaryChannel, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null || channel == null)
        {
            return Fail(error);
        }

        if (targetUser.Id == user.Id)
        {
            return Fail("频道拥有者已经拥有频道权限.");
        }

        var allowedUser = await _dataProvider.GetAllowedUserAsync(temporaryChannel.Id, targetUser.Id, cancellationToken).ConfigureAwait(false);
        if (allowedUser == null)
        {
            await _dataProvider.AddAllowedUserAsync(new DiscordTemporaryVoiceChannelAllowedUser
            {
                TemporaryVoiceChannelId = temporaryChannel.Id,
                GuildId = temporaryChannel.GuildId,
                ChannelId = temporaryChannel.ChannelId,
                UserId = targetUser.Id
            }, cancellationToken).ConfigureAwait(false);
        }

        await channel.AddPermissionOverwriteAsync(targetUser, AllowVoiceAccessOverwrite()).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok($"已允许 {targetUser.DisplayName} 查看和加入频道.");
    }

    public async Task<DiscordVoiceCommandResult> DenyUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default)
    {
        var (temporaryChannel, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null || channel == null)
        {
            return Fail(error);
        }

        if (targetUser.Id == user.Id)
        {
            return Fail("不能移除频道拥有者权限.");
        }

        var allowedUser = await _dataProvider.GetAllowedUserAsync(temporaryChannel.Id, targetUser.Id, cancellationToken).ConfigureAwait(false);
        if (allowedUser != null)
        {
            await _dataProvider.DeleteAllowedUserAsync(allowedUser, cancellationToken).ConfigureAwait(false);
        }

        await channel.RemovePermissionOverwriteAsync(targetUser).ConfigureAwait(false);
        if (targetUser.VoiceChannel?.Id == channel.Id)
        {
            await targetUser.ModifyAsync(properties => properties.Channel = null).ConfigureAwait(false);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok($"已移除 {targetUser.DisplayName} 的允许权限并踢出频道.");
    }

    public async Task<DiscordVoiceCommandResult> KickUserAsync(SocketGuildUser user, SocketGuildUser targetUser, CancellationToken cancellationToken = default)
    {
        var (_, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (channel == null)
        {
            return Fail(error);
        }

        if (targetUser.Id == user.Id)
        {
            return Fail("不能踢出频道拥有者，请使用 /voice delete 删除频道.");
        }

        if (targetUser.VoiceChannel?.Id != channel.Id)
        {
            return Fail("目标用户不在你的专属频道内.");
        }

        await targetUser.ModifyAsync(properties => properties.Channel = null).ConfigureAwait(false);

        return Ok($"已踢出 {targetUser.DisplayName}.");
    }

    public async Task<DiscordVoiceCommandResult> DeleteOwnChannelAsync(SocketGuildUser user, CancellationToken cancellationToken = default)
    {
        var (temporaryChannel, channel, error) = await GetOwnedActiveChannelAsync(user, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null)
        {
            return Fail(error);
        }

        if (channel != null)
        {
            await channel.DeleteAsync().ConfigureAwait(false);
        }

        temporaryChannel.DeletedAt = DateTimeOffset.UtcNow;
        temporaryChannel.LastEmptyAt = null;

        await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

        return Ok("专属语音频道已删除.");
    }

    public async Task<DiscordVoiceCommandResult> GetInfoAsync(SocketGuildUser user, CancellationToken cancellationToken = default)
    {
        var temporaryChannel = await _dataProvider.GetActiveTemporaryChannelByOwnerAsync(user.Guild.Id, user.Id, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null && user.VoiceChannel != null)
        {
            temporaryChannel = await _dataProvider.GetActiveTemporaryChannelByChannelAsync(user.Guild.Id, user.VoiceChannel.Id, cancellationToken).ConfigureAwait(false);
        }

        if (temporaryChannel == null)
        {
            return Fail("未找到相关专属语音频道.");
        }

        var allowedUsers = await _dataProvider.GetAllowedUsersAsync(temporaryChannel.Id, cancellationToken).ConfigureAwait(false);
        var emptyDeleteAt = temporaryChannel.LastEmptyAt?.Add(EmptyChannelLifetime);
        var privacy = temporaryChannel.IsPrivate ? "私密" : "公开";
        var allowedUserText = allowedUsers.Count == 0
            ? "无"
            : string.Join(", ", allowedUsers.Select(x => $"<@{x.UserId}>"));
        var emptyDeleteText = emptyDeleteAt == null
            ? "未计时"
            : $"{emptyDeleteAt.Value:yyyy-MM-dd HH:mm:ss zzz}";

        return Ok($"""
                  频道: <#{temporaryChannel.ChannelId}>
                  拥有者: <@{temporaryChannel.OwnerUserId}>
                  人数上限: {temporaryChannel.UserLimit}
                  状态: {privacy}
                  允许用户: {allowedUserText}
                  空频道删除时间: {emptyDeleteText}
                  """);
    }

    public async Task CleanupExpiredEmptyChannelsAsync(CancellationToken cancellationToken = default)
    {
        var emptyBefore = DateTimeOffset.UtcNow.Subtract(EmptyChannelLifetime);
        var temporaryChannels = await _dataProvider.GetExpiredEmptyTemporaryChannelsAsync(emptyBefore, cancellationToken).ConfigureAwait(false);

        foreach (var temporaryChannel in temporaryChannels)
        {
            var guild = _client.GetGuild(temporaryChannel.GuildId);
            var channel = guild?.GetVoiceChannel(temporaryChannel.ChannelId);
            if (channel == null)
            {
                temporaryChannel.DeletedAt = DateTimeOffset.UtcNow;
                temporaryChannel.LastEmptyAt = null;
                await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
                continue;
            }

            if (HasConnectedHumanUsers(channel))
            {
                temporaryChannel.LastEmptyAt = null;
                await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
                continue;
            }

            try
            {
                await channel.DeleteAsync().ConfigureAwait(false);
                temporaryChannel.DeletedAt = DateTimeOffset.UtcNow;
                temporaryChannel.LastEmptyAt = null;
                await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Failed to delete expired Discord temporary voice channel {ChannelId}", temporaryChannel.ChannelId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task MoveUserToOwnedOrNewChannelAsync(SocketGuildUser user, DiscordVoiceChannelConfig config, CancellationToken cancellationToken)
    {
        var activeChannel = await _dataProvider.GetActiveTemporaryChannelByOwnerAsync(config.GuildId, user.Id, cancellationToken).ConfigureAwait(false);
        var guild = user.Guild;

        if (activeChannel != null)
        {
            var existingChannel = guild.GetVoiceChannel(activeChannel.ChannelId);
            if (existingChannel != null)
            {
                activeChannel.LastEmptyAt = null;
                await _dataProvider.UpdateTemporaryChannelAsync(activeChannel, cancellationToken).ConfigureAwait(false);
                await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
                await user.ModifyAsync(properties => properties.Channel = existingChannel).ConfigureAwait(false);
                return;
            }

            activeChannel.DeletedAt = DateTimeOffset.UtcNow;
            activeChannel.LastEmptyAt = null;
            await _dataProvider.UpdateTemporaryChannelAsync(activeChannel, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        }

        var userLimit = ClampUserLimit(config.DefaultUserLimit);
        var channelName = BuildChannelName(config.ChannelNameTemplate, user);
        var createdChannel = await guild.CreateVoiceChannelAsync(channelName, properties =>
        {
            properties.CategoryId = config.CreateCategoryId;
            properties.UserLimit = userLimit;
        }).ConfigureAwait(false);

        await createdChannel.AddPermissionOverwriteAsync(guild.EveryoneRole, DenyVoiceAccessOverwrite()).ConfigureAwait(false);
        await createdChannel.AddPermissionOverwriteAsync(user, AllowVoiceAccessOverwrite()).ConfigureAwait(false);
        await createdChannel.AddPermissionOverwriteAsync(guild.CurrentUser, BotManageOverwrite()).ConfigureAwait(false);

        var temporaryChannel = new DiscordTemporaryVoiceChannel
        {
            GuildId = guild.Id,
            ConfigId = config.Id,
            TriggerChannelId = config.TriggerChannelId,
            CreateCategoryId = config.CreateCategoryId,
            ChannelId = createdChannel.Id,
            OwnerUserId = user.Id,
            OwnerDisplayName = user.DisplayName,
            UserLimit = userLimit,
            IsPrivate = true
        };

        await _dataProvider.CreateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        await user.ModifyAsync(properties => properties.Channel = createdChannel).ConfigureAwait(false);
    }

    private async Task RefreshTemporaryChannelEmptyStateAsync(SocketVoiceChannel? channel, CancellationToken cancellationToken)
    {
        if (channel == null)
        {
            return;
        }

        var temporaryChannel = await _dataProvider.GetActiveTemporaryChannelByChannelAsync(channel.Guild.Id, channel.Id, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null)
        {
            return;
        }

        var isEmpty = !HasConnectedHumanUsers(channel);
        if (isEmpty && temporaryChannel.LastEmptyAt == null)
        {
            temporaryChannel.LastEmptyAt = DateTimeOffset.UtcNow;
        }
        else if (!isEmpty && temporaryChannel.LastEmptyAt != null)
        {
            temporaryChannel.LastEmptyAt = null;
        }
        else
        {
            return;
        }

        await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
        await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    private async Task ApplyPrivacyOverwriteAsync(SocketVoiceChannel channel, DiscordTemporaryVoiceChannel temporaryChannel, CancellationToken cancellationToken)
    {
        var guild = channel.Guild;
        var everyoneOverwrite = temporaryChannel.IsPrivate ? DenyVoiceAccessOverwrite() : PublicVoiceAccessOverwrite();

        await channel.AddPermissionOverwriteAsync(guild.EveryoneRole, everyoneOverwrite).ConfigureAwait(false);

        var owner = guild.GetUser(temporaryChannel.OwnerUserId);
        if (owner != null)
        {
            await channel.AddPermissionOverwriteAsync(owner, AllowVoiceAccessOverwrite()).ConfigureAwait(false);
        }

        await channel.AddPermissionOverwriteAsync(guild.CurrentUser, BotManageOverwrite()).ConfigureAwait(false);

        if (!temporaryChannel.IsPrivate)
        {
            return;
        }

        var allowedUsers = await _dataProvider.GetAllowedUsersAsync(temporaryChannel.Id, cancellationToken).ConfigureAwait(false);
        var allowedUserIds = allowedUsers.Select(x => x.UserId).ToHashSet();
        foreach (var allowedUser in allowedUsers)
        {
            var guildUser = guild.GetUser(allowedUser.UserId);
            if (guildUser != null)
            {
                await channel.AddPermissionOverwriteAsync(guildUser, AllowVoiceAccessOverwrite()).ConfigureAwait(false);
            }
        }

        var connectedUsersToKick = channel.ConnectedUsers
            .Where(x =>
                !x.IsBot &&
                x.Id != temporaryChannel.OwnerUserId &&
                !allowedUserIds.Contains(x.Id))
            .ToList();

        foreach (var connectedUser in connectedUsersToKick)
        {
            await connectedUser.ModifyAsync(properties => properties.Channel = null).ConfigureAwait(false);
        }
    }

    private async Task<(DiscordTemporaryVoiceChannel? TemporaryChannel, SocketVoiceChannel? Channel, string Error)> GetOwnedActiveChannelAsync(
        SocketGuildUser user,
        CancellationToken cancellationToken)
    {
        var temporaryChannel = await _dataProvider.GetActiveTemporaryChannelByOwnerAsync(user.Guild.Id, user.Id, cancellationToken).ConfigureAwait(false);
        if (temporaryChannel == null)
        {
            return (null, null, "你还没有专属语音频道.");
        }

        var channel = user.Guild.GetVoiceChannel(temporaryChannel.ChannelId);
        if (channel == null)
        {
            temporaryChannel.DeletedAt = DateTimeOffset.UtcNow;
            temporaryChannel.LastEmptyAt = null;
            await _dataProvider.UpdateTemporaryChannelAsync(temporaryChannel, cancellationToken).ConfigureAwait(false);
            await _unitOfWork.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

            return (temporaryChannel, null, "专属语音频道已不存在，记录已清理.");
        }

        return (temporaryChannel, channel, string.Empty);
    }

    private async Task<string> ValidateConfigAsync(ulong guildId, ulong triggerChannelId, ulong createCategoryId, int defaultUserLimit, Guid? exceptId, CancellationToken cancellationToken)
    {
        if (guildId == 0)
        {
            return "GuildId is required.";
        }

        if (triggerChannelId == 0)
        {
            return "TriggerChannelId is required.";
        }

        if (createCategoryId == 0)
        {
            return "CreateCategoryId is required.";
        }

        if (defaultUserLimit is < MinUserLimit or > MaxUserLimit)
        {
            return $"DefaultUserLimit must be between {MinUserLimit} and {MaxUserLimit}.";
        }

        var exists = await _dataProvider.ConfigExistsAsync(guildId, triggerChannelId, exceptId, cancellationToken).ConfigureAwait(false);
        return exists ? "Voice channel config already exists for this guild and trigger channel." : string.Empty;
    }

    private static bool HasConnectedHumanUsers(SocketVoiceChannel channel)
    {
        return channel.ConnectedUsers.Any(x => !x.IsBot);
    }

    private static int ClampUserLimit(int userLimit)
    {
        return Math.Clamp(userLimit, MinUserLimit, MaxUserLimit);
    }

    private static string NormalizeTemplate(string template)
    {
        return string.IsNullOrWhiteSpace(template) ? "{user} 的频道" : template.Trim();
    }

    private static string BuildChannelName(string template, SocketGuildUser user)
    {
        var name = NormalizeTemplate(template)
            .Replace("{user}", user.DisplayName, StringComparison.OrdinalIgnoreCase)
            .Replace("{username}", user.Username, StringComparison.OrdinalIgnoreCase)
            .Replace("{id}", user.Id.ToString(), StringComparison.OrdinalIgnoreCase)
            .Replace("\r", " ", StringComparison.Ordinal)
            .Replace("\n", " ", StringComparison.Ordinal)
            .Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            name = $"{user.DisplayName} 的频道";
        }

        return name.Length <= 90 ? name : name[..90];
    }

    private static OverwritePermissions DenyVoiceAccessOverwrite()
    {
        return OverwritePermissions.InheritAll.Modify(
            viewChannel: PermValue.Deny,
            connect: PermValue.Deny);
    }

    private static OverwritePermissions PublicVoiceAccessOverwrite()
    {
        return OverwritePermissions.InheritAll.Modify(
            viewChannel: PermValue.Allow,
            connect: PermValue.Allow);
    }

    private static OverwritePermissions AllowVoiceAccessOverwrite()
    {
        return OverwritePermissions.InheritAll.Modify(
            viewChannel: PermValue.Allow,
            connect: PermValue.Allow,
            speak: PermValue.Allow);
    }

    private static OverwritePermissions BotManageOverwrite()
    {
        return OverwritePermissions.InheritAll.Modify(
            viewChannel: PermValue.Allow,
            manageChannel: PermValue.Allow,
            connect: PermValue.Allow,
            speak: PermValue.Allow,
            moveMembers: PermValue.Allow);
    }

    private static DiscordVoiceCommandResult Ok(string message)
    {
        return new DiscordVoiceCommandResult
        {
            Success = true,
            Message = message
        };
    }

    private static DiscordVoiceCommandResult Fail(string message)
    {
        return new DiscordVoiceCommandResult
        {
            Message = message
        };
    }
}
