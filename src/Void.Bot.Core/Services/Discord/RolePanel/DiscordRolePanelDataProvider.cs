using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Discord;

namespace Void.Bot.Core.Services.Discord.RolePanel;

public interface IDiscordRolePanelDataProvider : IScope
{
    Task<DiscordRolePanel?> GetAsync(ulong guildId, CancellationToken cancellationToken);
    Task<DiscordRolePanel?> GetByMessageAsync(ulong guildId, ulong messageId, CancellationToken cancellationToken);
    Task CreateAsync(DiscordRolePanel panel, CancellationToken cancellationToken);
    Task UpdateAsync(DiscordRolePanel panel, CancellationToken cancellationToken);
    Task DeleteAsync(DiscordRolePanel panel, CancellationToken cancellationToken);
}

public sealed class DiscordRolePanelDataProvider(
    IRepository<DiscordRolePanel> panelRepository) : IDiscordRolePanelDataProvider
{
    public Task<DiscordRolePanel?> GetAsync(
        ulong guildId,
        CancellationToken cancellationToken)
    {
        return panelRepository.Query(x => x.GuildId == guildId)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task<DiscordRolePanel?> GetByMessageAsync(
        ulong guildId,
        ulong messageId,
        CancellationToken cancellationToken)
    {
        return panelRepository.Query(x => x.GuildId == guildId && x.MessageId == messageId)
            .Include(x => x.Items)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public Task CreateAsync(DiscordRolePanel panel, CancellationToken cancellationToken)
    {
        return panelRepository.InsertAsync(panel, cancellationToken);
    }

    public Task UpdateAsync(DiscordRolePanel panel, CancellationToken cancellationToken)
    {
        return panelRepository.UpdateAsync(panel, cancellationToken);
    }

    public Task DeleteAsync(DiscordRolePanel panel, CancellationToken cancellationToken)
    {
        return panelRepository.DeleteAsync(panel, cancellationToken);
    }
}
