using Microsoft.EntityFrameworkCore;
using Void.Bot.Core.Data;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Domain.ERA;

namespace Void.Bot.Core.Services.ERA.Player;

public interface IEraDataProvider : IScope
{
    Task<List<EraConfiguration>> GetConfigurationsAsync(
        bool forUpdate,
        CancellationToken cancellationToken);
    Task<bool> LockAccountAsync(Guid accountId, CancellationToken cancellationToken);
    Task<EraPlayer?> GetPlayerByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task<EraPlayer?> LockPlayerByAccountIdAsync(Guid accountId, CancellationToken cancellationToken);
    Task<EraPlayer?> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<EraPlayer?> LockPlayerAsync(Guid playerId, CancellationToken cancellationToken);
    Task<List<EraCharacter>> GetCharactersByPlayerIdAsync(Guid playerId, CancellationToken cancellationToken);
    Task<EraCharacter?> LockCharacterAsync(Guid characterId, CancellationToken cancellationToken);
    Task<EraCharacter?> GetCharacterByIdAsync(Guid characterId, CancellationToken cancellationToken);
    Task<EraCharacter?> GetCharacterByCreationRequestIdAsync(
        Guid playerId,
        Guid requestId,
        CancellationToken cancellationToken);
    Task<EraCapacityChange?> GetReceiptAsync(Guid playerId, Guid requestId, CancellationToken cancellationToken);
    Task CreatePlayerAsync(EraPlayer player, CancellationToken cancellationToken);
    Task CreateConfigurationsAsync(
        IReadOnlyCollection<EraConfiguration> configurations,
        CancellationToken cancellationToken);
    void RemoveConfigurations(IReadOnlyCollection<EraConfiguration> configurations);
    Task CreatePlayerAccountLinkAsync(
        EraPlayerAccountLink playerAccountLink,
        CancellationToken cancellationToken);
    Task CreateCharacterAsync(EraCharacter character, CancellationToken cancellationToken);
    Task CreateReceiptAsync(EraCapacityChange receipt, CancellationToken cancellationToken);
}

public class EraDataProvider : IEraDataProvider
{
    private readonly IRepository<EraConfiguration> _eraConfigurationRepository;
    private readonly IRepository<UserAccount> _userAccountRepository;
    private readonly IRepository<EraPlayer> _eraPlayerRepository;
    private readonly IRepository<EraPlayerAccountLink> _eraPlayerAccountLinkRepository;
    private readonly IRepository<EraCharacter> _eraCharacterRepository;
    private readonly IRepository<EraCapacityChange> _eraCapacityChangeRepository;

    public EraDataProvider(
        IRepository<EraConfiguration> eraConfigurationRepository,
        IRepository<UserAccount> userAccountRepository,
        IRepository<EraPlayer> eraPlayerRepository,
        IRepository<EraPlayerAccountLink> eraPlayerAccountLinkRepository,
        IRepository<EraCharacter> eraCharacterRepository,
        IRepository<EraCapacityChange> eraCapacityChangeRepository)
    {
        _eraConfigurationRepository = eraConfigurationRepository;
        _userAccountRepository = userAccountRepository;
        _eraPlayerRepository = eraPlayerRepository;
        _eraPlayerAccountLinkRepository = eraPlayerAccountLinkRepository;
        _eraCharacterRepository = eraCharacterRepository;
        _eraCapacityChangeRepository = eraCapacityChangeRepository;
    }

    public async Task<List<EraConfiguration>> GetConfigurationsAsync(
        bool forUpdate,
        CancellationToken cancellationToken)
    {
        if (!forUpdate)
        {
            return await _eraConfigurationRepository.QueryNoTracking()
                .OrderBy(configuration => configuration.ConfigKey)
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        // Command 的事务由 Mediator 管道开启；锁定全部当前配置项以串行化整套配置发布。
        return await _eraConfigurationRepository.QuerySqlRaw("SELECT * FROM era_configuration ORDER BY config_key FOR UPDATE")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> LockAccountAsync(Guid accountId, CancellationToken cancellationToken)
    {
        // 锁已存在的账号行，串行化同一账号首次初始化，避免先查后建的竞争。
        var rows = await _userAccountRepository.QuerySql($"SELECT * FROM user_account WHERE id = {accountId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.Count == 1;
    }

    public async Task<EraPlayer?> GetPlayerByIdAsync(Guid playerId, CancellationToken cancellationToken)
    {
        return await _eraPlayerRepository.QueryNoTracking()
            .SingleOrDefaultAsync(player => player.Id == playerId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraPlayer?> GetPlayerByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var playerAccountLink = await _eraPlayerAccountLinkRepository.QueryNoTracking()
            .SingleOrDefaultAsync(link => link.AccountId == accountId, cancellationToken)
            .ConfigureAwait(false);
        if (playerAccountLink == null)
        {
            return null;
        }

        return await _eraPlayerRepository.QueryNoTracking()
            .SingleOrDefaultAsync(player => player.Id == playerAccountLink.PlayerId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraPlayer?> LockPlayerByAccountIdAsync(
        Guid accountId,
        CancellationToken cancellationToken)
    {
        var rows = await _eraPlayerRepository.QuerySql(
                $"SELECT era_player.* FROM era_player INNER JOIN era_player_account_link ON era_player_account_link.player_id = era_player.id WHERE era_player_account_link.account_id = {accountId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<EraPlayer?> LockPlayerAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var rows = await _eraPlayerRepository.QuerySql($"SELECT * FROM era_player WHERE id = {playerId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task<List<EraCharacter>> GetCharactersByPlayerIdAsync(
        Guid playerId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterRepository.QueryNoTracking()
            .Where(character => character.PlayerId == playerId)
            .OrderBy(character => character.CreatedAt)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraCharacter?> LockCharacterAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        var rows = await _eraCharacterRepository.QuerySql($"SELECT * FROM era_character WHERE id = {characterId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);
        return rows.SingleOrDefault();
    }
    public async Task<EraCharacter?> GetCharacterByIdAsync(
        Guid characterId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterRepository.QueryNoTracking()
            .SingleOrDefaultAsync(character => character.Id == characterId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraCharacter?> GetCharacterByCreationRequestIdAsync(
        Guid playerId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        return await _eraCharacterRepository.Query()
            .SingleOrDefaultAsync(
                character => character.PlayerId == playerId && character.CreationRequestId == requestId,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<EraCapacityChange?> GetReceiptAsync(
        Guid playerId,
        Guid requestId,
        CancellationToken cancellationToken)
    {
        // 锁定读取使用当前已提交结果，不依赖 MySQL 可重复读事务中的旧查询快照。
        var rows = await _eraCapacityChangeRepository.QuerySql(
                $"SELECT * FROM era_capacity_change WHERE player_id = {playerId.ToString()} AND request_id = {requestId.ToString()} FOR UPDATE")
            .ToListAsync(cancellationToken).ConfigureAwait(false);
        return rows.SingleOrDefault();
    }

    public async Task CreatePlayerAsync(EraPlayer player, CancellationToken cancellationToken)
    {
        await _eraPlayerRepository.InsertAsync(player, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateConfigurationsAsync(
        IReadOnlyCollection<EraConfiguration> configurations,
        CancellationToken cancellationToken)
    {
        await _eraConfigurationRepository.InsertManyAsync(configurations, cancellationToken)
            .ConfigureAwait(false);
    }

    public void RemoveConfigurations(IReadOnlyCollection<EraConfiguration> configurations)
    {
        _eraConfigurationRepository.DeleteMany(configurations);
    }

    public async Task CreatePlayerAccountLinkAsync(
        EraPlayerAccountLink playerAccountLink,
        CancellationToken cancellationToken)
    {
        await _eraPlayerAccountLinkRepository.InsertAsync(playerAccountLink, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task CreateCharacterAsync(EraCharacter character, CancellationToken cancellationToken)
    {
        await _eraCharacterRepository.InsertAsync(character, cancellationToken).ConfigureAwait(false);
    }

    public async Task CreateReceiptAsync(EraCapacityChange receipt, CancellationToken cancellationToken)
    {
        await _eraCapacityChangeRepository.InsertAsync(receipt, cancellationToken).ConfigureAwait(false);
    }
}
