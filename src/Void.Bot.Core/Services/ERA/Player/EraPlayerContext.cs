using System.Net;
using Void.Bot.Core.Domain.ERA;
using Void.Bot.Core.Exceptions;
using Void.Bot.Core.Resources.i18n.ERA;
using Void.Bot.Core.Services.ERA.Player.Models;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Services.Language;

namespace Void.Bot.Core.Services.ERA.Player;

/// <summary>
/// 请求作用域内的玩家加载入口；缓存已验证的玩家，并支持读操作升级为行锁。
/// </summary>
public interface IEraPlayerContext : IScope
{
    /// <summary>
    /// 取得当前账号的玩家；forUpdate 为 true 时取得玩家行锁。
    /// </summary>
    Task<EraPlayer> RequireCurrentAsync(
        bool forUpdate,
        CancellationToken cancellationToken);

    /// <summary>
    /// 取得指定目标玩家；缺失时由统一入口抛出玩家不存在的业务异常。
    /// </summary>
    Task<EraPlayer> RequireTargetAsync(
        Guid playerId,
        bool forUpdate,
        CancellationToken cancellationToken);
}

public class EraPlayerContext : IEraPlayerContext
{
    private readonly IEraDataProvider _dataProvider;
    private readonly ICurrentUser _currentUser;
    private readonly ILocalizationService _localizationService;
    private readonly EraPlayerData _playerData;

    public EraPlayerContext(
        IEraDataProvider dataProvider,
        ICurrentUser currentUser,
        ILocalizationService localizationService,
        EraPlayerData playerData)
    {
        _dataProvider = dataProvider;
        _currentUser = currentUser;
        _localizationService = localizationService;
        _playerData = playerData;
    }

    public async Task<EraPlayer> RequireCurrentAsync(bool forUpdate, CancellationToken cancellationToken)
    {
        if (!_currentUser.Id.HasValue || _currentUser.Id.Value == Guid.Empty)
        {
            throw new EraPlayerAccessException(
                HttpStatusCode.Unauthorized,
                "era.unauthorized",
                _localizationService.GetValue<EraContextTemplate>(text => text.Unauthorized));
        }

        if (_playerData.CurrentPlayer != null)
        {
            return await RequireTargetAsync(_playerData.CurrentPlayer.Id, forUpdate, cancellationToken)
                .ConfigureAwait(false);
        }

        EraPlayer? player;
        if (forUpdate)
        {
            player = await _dataProvider.LockPlayerByAccountIdAsync(_currentUser.Id.Value, cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            player = await _dataProvider.GetPlayerByAccountIdAsync(_currentUser.Id.Value, cancellationToken)
                .ConfigureAwait(false);
        }

        var loadedPlayer = StorePlayer(player, forUpdate);
        _playerData.CurrentPlayer = loadedPlayer;
        return loadedPlayer;
    }

    public async Task<EraPlayer> RequireTargetAsync(Guid playerId, bool forUpdate, CancellationToken cancellationToken)
    {
        if (_playerData.Players.TryGetValue(playerId, out var cachedPlayer) &&
            (!forUpdate || _playerData.LockedPlayers.Contains(playerId)))
        {
            return cachedPlayer;
        }

        EraPlayer? player;
        if (forUpdate)
        {
            player = await _dataProvider.LockPlayerAsync(playerId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            player = await _dataProvider.GetPlayerByIdAsync(playerId, cancellationToken).ConfigureAwait(false);
        }

        return StorePlayer(player, forUpdate);
    }

    private EraPlayer StorePlayer(EraPlayer? player, bool forUpdate)
    {
        if (player == null)
        {
            throw new EraPlayerAccessException(
                HttpStatusCode.NotFound,
                "era.player_not_found",
                _localizationService.GetValue<EraContextTemplate>(text => text.PlayerNotFound));
        }

        _playerData.Players[player.Id] = player;
        if (forUpdate)
        {
            _playerData.LockedPlayers.Add(player.Id);
        }

        if (_playerData.CurrentPlayer?.Id == player.Id)
        {
            _playerData.CurrentPlayer = player;
        }

        return player;
    }
}
