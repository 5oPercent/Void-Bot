using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Discord;

namespace Void.Bot.Core.Services.Discord.DataProvider;

public interface IDiscordVoiceChannelDataProvider : IScope
{
    Task<DiscordVoiceChannelConfig?> GetConfigAsync(Guid id, CancellationToken cancellationToken);

    Task<DiscordVoiceChannelConfig?> GetConfigAsync(ulong guildId, ulong triggerChannelId, CancellationToken cancellationToken);

    Task<List<DiscordVoiceChannelConfig>> GetConfigsAsync(ulong? guildId, CancellationToken cancellationToken);

    Task<bool> ConfigExistsAsync(ulong guildId, ulong triggerChannelId, Guid? exceptId, CancellationToken cancellationToken);

    Task CreateConfigAsync(DiscordVoiceChannelConfig entity, CancellationToken cancellationToken);

    Task UpdateConfigAsync(DiscordVoiceChannelConfig entity, CancellationToken cancellationToken);

    Task DeleteConfigsAsync(List<Guid> idList, CancellationToken cancellationToken);

    Task<DiscordTemporaryVoiceChannel?> GetActiveTemporaryChannelByOwnerAsync(ulong guildId, ulong ownerUserId, CancellationToken cancellationToken);

    Task<DiscordTemporaryVoiceChannel?> GetActiveTemporaryChannelByChannelAsync(ulong guildId, ulong channelId, CancellationToken cancellationToken);

    Task<List<DiscordTemporaryVoiceChannel>> GetTemporaryChannelsAsync(ulong? guildId, bool includeDeleted, CancellationToken cancellationToken);

    Task<List<DiscordTemporaryVoiceChannel>> GetExpiredEmptyTemporaryChannelsAsync(DateTimeOffset emptyBefore, CancellationToken cancellationToken);

    Task CreateTemporaryChannelAsync(DiscordTemporaryVoiceChannel entity, CancellationToken cancellationToken);

    Task UpdateTemporaryChannelAsync(DiscordTemporaryVoiceChannel entity, CancellationToken cancellationToken);

    Task<List<DiscordTemporaryVoiceChannelAllowedUser>> GetAllowedUsersAsync(Guid temporaryVoiceChannelId, CancellationToken cancellationToken);

    Task<List<DiscordTemporaryVoiceChannelAllowedUser>> GetAllowedUsersAsync(List<Guid> temporaryVoiceChannelIds, CancellationToken cancellationToken);

    Task<DiscordTemporaryVoiceChannelAllowedUser?> GetAllowedUserAsync(Guid temporaryVoiceChannelId, ulong userId, CancellationToken cancellationToken);

    Task AddAllowedUserAsync(DiscordTemporaryVoiceChannelAllowedUser entity, CancellationToken cancellationToken);

    Task DeleteAllowedUserAsync(DiscordTemporaryVoiceChannelAllowedUser entity, CancellationToken cancellationToken);
}

public class DiscordVoiceChannelDataProvider : IDiscordVoiceChannelDataProvider
{
    private readonly IRepository<DiscordVoiceChannelConfig> _configRep;
    private readonly IRepository<DiscordTemporaryVoiceChannel> _temporaryChannelRep;
    private readonly IRepository<DiscordTemporaryVoiceChannelAllowedUser> _allowedUserRep;

    public DiscordVoiceChannelDataProvider(
        IRepository<DiscordVoiceChannelConfig> configRep,
        IRepository<DiscordTemporaryVoiceChannel> temporaryChannelRep,
        IRepository<DiscordTemporaryVoiceChannelAllowedUser> allowedUserRep)
    {
        _configRep = configRep;
        _temporaryChannelRep = temporaryChannelRep;
        _allowedUserRep = allowedUserRep;
    }

    public async Task<DiscordVoiceChannelConfig?> GetConfigAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _configRep.Query(x => x.Id == id).FirstOrDefaultAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<DiscordVoiceChannelConfig?> GetConfigAsync(ulong guildId, ulong triggerChannelId, CancellationToken cancellationToken)
    {
        return await _configRep.QueryNoTracking(x =>
                x.GuildId == guildId &&
                x.TriggerChannelId == triggerChannelId &&
                x.Enabled)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<DiscordVoiceChannelConfig>> GetConfigsAsync(ulong? guildId, CancellationToken cancellationToken)
    {
        var query = _configRep.QueryNoTracking();
        if (guildId.HasValue)
        {
            query = query.Where(x => x.GuildId == guildId.Value);
        }

        return await query
            .OrderBy(x => x.GuildId)
            .ThenBy(x => x.TriggerChannelId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> ConfigExistsAsync(ulong guildId, ulong triggerChannelId, Guid? exceptId, CancellationToken cancellationToken)
    {
        return await _configRep.AnyAsync(x =>
                x.GuildId == guildId &&
                x.TriggerChannelId == triggerChannelId &&
                (!exceptId.HasValue || x.Id != exceptId.Value),
            cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateConfigAsync(DiscordVoiceChannelConfig entity, CancellationToken cancellationToken)
    {
        await _configRep.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateConfigAsync(DiscordVoiceChannelConfig entity, CancellationToken cancellationToken)
    {
        await _configRep.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteConfigsAsync(List<Guid> idList, CancellationToken cancellationToken)
    {
        await _configRep.DeleteAsync(x => idList.Contains(x.Id), cancellationToken).ConfigureAwait(false);
    }

    public async Task<DiscordTemporaryVoiceChannel?> GetActiveTemporaryChannelByOwnerAsync(ulong guildId, ulong ownerUserId, CancellationToken cancellationToken)
    {
        return await _temporaryChannelRep.Query(x =>
                x.GuildId == guildId &&
                x.OwnerUserId == ownerUserId &&
                x.DeletedAt == null)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<DiscordTemporaryVoiceChannel?> GetActiveTemporaryChannelByChannelAsync(ulong guildId, ulong channelId, CancellationToken cancellationToken)
    {
        return await _temporaryChannelRep.Query(x =>
                x.GuildId == guildId &&
                x.ChannelId == channelId &&
                x.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<DiscordTemporaryVoiceChannel>> GetTemporaryChannelsAsync(ulong? guildId, bool includeDeleted, CancellationToken cancellationToken)
    {
        var query = _temporaryChannelRep.QueryNoTracking();
        if (guildId.HasValue)
        {
            query = query.Where(x => x.GuildId == guildId.Value);
        }

        if (!includeDeleted)
        {
            query = query.Where(x => x.DeletedAt == null);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<DiscordTemporaryVoiceChannel>> GetExpiredEmptyTemporaryChannelsAsync(DateTimeOffset emptyBefore, CancellationToken cancellationToken)
    {
        return await _temporaryChannelRep.Query(x =>
                x.DeletedAt == null &&
                x.LastEmptyAt != null &&
                x.LastEmptyAt <= emptyBefore)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateTemporaryChannelAsync(DiscordTemporaryVoiceChannel entity, CancellationToken cancellationToken)
    {
        await _temporaryChannelRep.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task UpdateTemporaryChannelAsync(DiscordTemporaryVoiceChannel entity, CancellationToken cancellationToken)
    {
        await _temporaryChannelRep.UpdateAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<DiscordTemporaryVoiceChannelAllowedUser>> GetAllowedUsersAsync(Guid temporaryVoiceChannelId, CancellationToken cancellationToken)
    {
        return await _allowedUserRep.QueryNoTracking(x => x.TemporaryVoiceChannelId == temporaryVoiceChannelId)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<List<DiscordTemporaryVoiceChannelAllowedUser>> GetAllowedUsersAsync(List<Guid> temporaryVoiceChannelIds, CancellationToken cancellationToken)
    {
        return await _allowedUserRep.QueryNoTracking(x => temporaryVoiceChannelIds.Contains(x.TemporaryVoiceChannelId))
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<DiscordTemporaryVoiceChannelAllowedUser?> GetAllowedUserAsync(Guid temporaryVoiceChannelId, ulong userId, CancellationToken cancellationToken)
    {
        return await _allowedUserRep.Query(x =>
                x.TemporaryVoiceChannelId == temporaryVoiceChannelId &&
                x.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task AddAllowedUserAsync(DiscordTemporaryVoiceChannelAllowedUser entity, CancellationToken cancellationToken)
    {
        await _allowedUserRep.InsertAsync(entity, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteAllowedUserAsync(DiscordTemporaryVoiceChannelAllowedUser entity, CancellationToken cancellationToken)
    {
        await _allowedUserRep.DeleteAsync(entity, cancellationToken).ConfigureAwait(false);
    }
}
