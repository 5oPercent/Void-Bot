using System.Runtime.ExceptionServices;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Serilog;
using Void.Bot.Core.Caching;
using Void.Bot.Core.Extension;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Core.Settings.Bungie;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Regexs;
using Void.Bot.Message.Request.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Middlewares.Preload;

public partial class LoadDestiny2PlayerSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private const string BungieIdCachePrefix = $"{Destiny2Constants.Destiny2}:BungieId";
    private const string PlayerCachePrefix = $"{Destiny2Constants.Destiny2}:Player";
    
    private readonly TimeSpan _expiryTime;

    private readonly ILogger _logger;
    private readonly ICurrentUser _currentUser;
    private readonly Destiny2PlayerData _playerData;
    private readonly IBungieService _bungieService;
    private readonly IAccountDataProvider _accountDataProvider;
    private readonly ICacheManager _cacheManager;

    public LoadDestiny2PlayerSpecification(
        ILogger logger,
        ICurrentUser currentUser,
        Destiny2PlayerData playerData,
        IBungieService bungieService,
        IAccountDataProvider accountDataProvider,
        ICacheManager cacheManager,
        Destiny2Setting destiny2Setting)
    {
        _logger = logger;
        _currentUser = currentUser;
        _playerData = playerData;
        _bungieService = bungieService;
        _accountDataProvider = accountDataProvider;
        _cacheManager = cacheManager;

        _expiryTime = new TimeSpan(0, destiny2Setting.CachePlayerDataMinutes, 0);
    }
    
    public bool ShouldExecute(TContext context, CancellationToken cancellationToken)
    {
        var message = context.Message;
        
        var isGetCurrentPlayerData = IsGetCurrentPlayerData(message);
        var isGetPlayerDataByNameCode = IsGetPlayerDataByNameCode(message);
        
        return isGetCurrentPlayerData || isGetPlayerDataByNameCode;
    }

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken))
            return;

        if (context.Message is IRequestDestiny2ByPlayerNameCode request)
        {
            var isGetCurrentPlayerData = IsGetCurrentPlayerData(context.Message);
            var isGetPlayerDataByNameCode = IsGetPlayerDataByNameCode(request);

            if (isGetPlayerDataByNameCode)
            {
                _playerData.GlobalDisplayName = request.DisplayName;
                _playerData.GlobalDisplayNameCode = request.DisplayNameCode;

                if (!BungieRegexs.UniqueName().IsMatch(_playerData.UniqueName))
                {
                    _logger.Warning("Invalid UniqueName: {UniqueName}", _playerData.UniqueName);
                    throw new ArgumentException("Invalid UniqueName.");
                }

                var playerCacheKey = $"{PlayerCachePrefix}:{_playerData.UniqueName}";
                var playerCache = await _cacheManager.Get<Destiny2PlayerData>(playerCacheKey, cancellationToken).ConfigureAwait(false);
                if (playerCache != null)
                {
                    _playerData.AssignFrom(playerCache);
                    ApplyPlayerProfile(_playerData, request.MembershipType);
                    PopulateMissingRequestFields(_playerData, request);
                    return;
                }

                await LoadPlayerProfilesAsync(_playerData, cancellationToken).ConfigureAwait(false);

                var membershipResult = await _bungieService.GetMembershipsByIdAsync(
                    new GetBungieMembershipsByIdRequest
                    {
                        MembershipType = _playerData.PrimaryMembershipType,
                        MembershipId = _playerData.PrimaryMembershipId
                    }, cancellationToken).ConfigureAwait(false);

                if (membershipResult.Data is { Response.BungieNetUser: not null })
                {
                    var bungieNetUser = membershipResult.Data.Response.BungieNetUser;

                    _playerData.BungieMembershipId = bungieNetUser.MembershipId;
                    _playerData.FirstAccess = bungieNetUser.FirstAccess;
                    _playerData.LastUpdate = bungieNetUser.LastUpdate;
                    _playerData.ShowActivity = bungieNetUser.ShowActivity;
                    _playerData.Language = bungieNetUser.Locale;
                }
            }
            else if (isGetCurrentPlayerData && _currentUser.Id.HasValue)
            {
                var thirdResult = await _accountDataProvider.GetThirdPartyByUserIdAsync(
                    _currentUser.Id.Value, nameof(AppPlatformEnum.Bungie), cancellationToken).ConfigureAwait(false);
                if (thirdResult == null)
                {
                    _logger.Warning("Not found third party {Platform} data for user {UserId}", AppPlatformEnum.Bungie, _currentUser.Id);
                    throw new ArgumentException($"Not found third party {nameof(AppPlatformEnum.Bungie)} data.");
                }

                _playerData.BungieMembershipId = thirdResult.ThirdPartyUserId;

                var playerUniqueNameCacheKey = $"{BungieIdCachePrefix}:{_playerData.BungieMembershipId}";
                var playerUniqueName = await _cacheManager.Get<string>(playerUniqueNameCacheKey, cancellationToken).ConfigureAwait(false);
                if (!string.IsNullOrEmpty(playerUniqueName))
                {
                    var playerCacheKey = $"{PlayerCachePrefix}:{playerUniqueName}";
                    var playerCache = await _cacheManager.Get<Destiny2PlayerData>(playerCacheKey, cancellationToken).ConfigureAwait(false);
                    if (playerCache != null)
                    {
                        _playerData.AssignFrom(playerCache);
                        ApplyPlayerProfile(_playerData, request.MembershipType);
                        PopulateMissingRequestFields(_playerData, request);
                        return;
                    }
                }

                var bungieNetUserResponse = await _bungieService.GetBungieNetUserAsync(
                    new GetBungieNetUserRequest
                    {
                        Id = _playerData.BungieMembershipId
                    }, cancellationToken).ConfigureAwait(false);

                if (bungieNetUserResponse.Data?.Response == null)
                {
                    _logger.Warning("Not found BungieNet user for membership ID {MembershipId}", _playerData.BungieMembershipId);
                    throw new ArgumentException("Not found BungieNet user.");
                }

                var bungieNetUser = bungieNetUserResponse.Data.Response;

                _playerData.GlobalDisplayName = bungieNetUser.CachedBungieGlobalDisplayName;
                _playerData.GlobalDisplayNameCode = bungieNetUser.CachedBungieGlobalDisplayNameCode.ToString().PadLeft(4, '0');
                _playerData.FirstAccess = bungieNetUser.FirstAccess;
                _playerData.LastUpdate = bungieNetUser.LastUpdate;
                _playerData.ShowActivity = bungieNetUser.ShowActivity;
                _playerData.Language = bungieNetUser.Locale;

                await LoadPlayerProfilesAsync(_playerData, cancellationToken).ConfigureAwait(false);

                await _cacheManager.Set(playerUniqueNameCacheKey, _playerData.UniqueName, _expiryTime, cancellationToken).ConfigureAwait(false);
            }
            else if (isGetCurrentPlayerData)
            {
                _logger.Warning("Current user is required to load Destiny 2 player data.");
                throw new ArgumentException("Current user is required to load Destiny 2 player data.");
            }

            var cacheKey = $"{PlayerCachePrefix}:{_playerData.UniqueName}";
            await _cacheManager.Set(cacheKey, _playerData, _expiryTime, cancellationToken).ConfigureAwait(false);
            
            ApplyPlayerProfile(_playerData, request.MembershipType);
            PopulateMissingRequestFields(_playerData, request);
        }
    }

    public Task Execute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task AfterExecute(TContext context, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task OnException(Exception ex, TContext context)
    {
        _playerData.Reset();
        
        ExceptionDispatchInfo.Capture(ex).Throw();
        return Task.CompletedTask;
    }

    private async Task LoadPlayerProfilesAsync(
        Destiny2PlayerData playerData, CancellationToken cancellationToken)
    {
        var profileResult = await _bungieService.GetDestiny2ProfileListByBungieNameAsync(
            new GetDestiny2ProfileListByBungieNameRequest
            {
                DisplayName = playerData.GlobalDisplayName,
                DisplayNameCode = playerData.GlobalDisplayNameCode,
                MembershipType = BungieMembershipTypeEnum.All
            }, cancellationToken).ConfigureAwait(false);

        if (profileResult.Data == null || profileResult.Data.Response.Count == 0)
        {
            throw new ArgumentException($"No Destiny 2 profiles found for {_playerData.UniqueName}");
        }
        
        var profiles = profileResult.Data.Response;
        
        playerData.Profiles = profiles.Select(x =>
            new Destiny2PlayerDataMembershipItem
            {
                MembershipType = x.MembershipType,
                MembershipId = x.MembershipId,
                DisplayName = x.DisplayName
            }).ToList();
        
        var crossSaveOverride = profiles
            .Select(x => x.CrossSaveOverride)
            .FirstOrDefault(x => x != BungieMembershipTypeEnum.None);

        if (crossSaveOverride != BungieMembershipTypeEnum.None)
        {
            playerData.CrossSaveOverride = crossSaveOverride;
        }
    }

    private void ApplyPlayerProfile(Destiny2PlayerData playerData, BungieMembershipTypeEnum membershipType)
    {
        var profiles = playerData.Profiles;

        Destiny2PlayerDataMembershipItem? profile = null;
        
        // 当指定平台时，优先使用指定平台
        if (membershipType != BungieMembershipTypeEnum.None)
        {
            profile = profiles.FirstOrDefault(x => x.MembershipType == membershipType);
        }
        // 未指定平台时，按规则选择
        else if (profiles.Count == 1)
        {
            profile = profiles[0];
        }
        // 多平台时, 选择跨平台数据
        else if (playerData.OnCrossSave)
        {
            profile = profiles.FirstOrDefault(x => x.MembershipType == playerData.CrossSaveOverride);
        }

        // 主数据为当前查询的平台
        if (profile == null)
        {
            Log.Warning("Not found Destiny 2 profile for {UniqueName} by {MembershipType} platform", playerData.UniqueName, membershipType);
            return;
        }
        
        playerData.PrimaryMembershipType = profile.MembershipType;
        playerData.PrimaryMembershipId = profile.MembershipId;
    }

    private static void PopulateMissingRequestFields(Destiny2PlayerData playerData, IRequestDestiny2ByPlayerNameCode request)
    {
        if (string.IsNullOrEmpty(request.DisplayName))
        {
            request.DisplayName = playerData.GlobalDisplayName;
        }
        if (string.IsNullOrEmpty(request.DisplayNameCode))
        {
            request.DisplayNameCode = playerData.GlobalDisplayNameCode;
        }
        if (request.MembershipType == BungieMembershipTypeEnum.None)
        {
            request.MembershipType = playerData.PrimaryMembershipType;
        }
    }

    private static bool IsGetCurrentPlayerData(IMessage message)
    {
        return message is ILoadDestiny2Player;
    }

    private static bool IsGetPlayerDataByNameCode(IMessage message)
    {
        return message is IRequestDestiny2ByPlayerNameCode request && IsGetPlayerDataByNameCode(request);
    }

    private static bool IsGetPlayerDataByNameCode(IRequestDestiny2ByPlayerNameCode request)
    {
        return !string.IsNullOrEmpty(request.DisplayName) || !string.IsNullOrEmpty(request.DisplayNameCode);
    }

}
