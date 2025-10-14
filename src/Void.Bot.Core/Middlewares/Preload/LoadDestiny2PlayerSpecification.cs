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
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Constants.Bungie.Destiny2;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Regexs;
using Void.Bot.Message.Request.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Middlewares.Preload;

public partial class LoadDestiny2PlayerSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private bool _isGetCurrentPlayerData = false;
    private bool _isGetPlayerDataByNameCode = false;
    
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
        
        _isGetCurrentPlayerData = message.GetType()
            .GetCustomAttributes(typeof(LoadDestiny2PlayerAttribute), inherit: true)
            .Length != 0;

        if (message is IRequestDestiny2ByPlayerNameCode dto)
        {
            _isGetPlayerDataByNameCode = !string.IsNullOrEmpty(dto.DisplayName) || !string.IsNullOrEmpty(dto.DisplayNameCode);
        }
        
        return _isGetCurrentPlayerData || _isGetPlayerDataByNameCode;
    }

    public async Task BeforeExecute(TContext context, CancellationToken cancellationToken)
    {
        if (!ShouldExecute(context, cancellationToken))
            return;

        var isOk = false;
        
        // 优先按参数查获取应玩家数据
        if (_isGetPlayerDataByNameCode && context.Message is IRequestDestiny2ByPlayerNameCode request)
        {
            _playerData.GlobalDisplayName = request.DisplayName;
            _playerData.GlobalDisplayNameCode = request.DisplayNameCode;

            if (!BungieRegexs.UniqueName().IsMatch(_playerData.UniqueName))
            {
                _logger.Warning("Invalid UniqueName: {UniqueName}", _playerData.UniqueName);
                goto Break;
            }
            
            var playerCache = await _cacheManager.Get<Destiny2PlayerData>(
                $"{PlayerCachePrefix}:{_playerData.UniqueName}", cancellationToken).ConfigureAwait(false);
            if (playerCache != null)
            {
                isOk = true;
                _playerData.AssignFrom(playerCache);
                goto Break;
            }

            if (!await LoadPlayerProfilesAsync(request.MembershipType, cancellationToken))
            {
                _logger.Warning("Load player profiles failed for {UniqueName}", _playerData.UniqueName);
                goto Break;
            }
            
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
                _playerData.ShowActivity =bungieNetUser.ShowActivity;
                _playerData.Language = bungieNetUser.Locale;
            }
                
            isOk = true;
        }
        else if (_isGetCurrentPlayerData && _currentUser.Id.HasValue) // 根据绑定的信息查询玩家数据
        {
            var thirdResult = await _accountDataProvider.GetThirdPartyByUserIdAsync(
                _currentUser.Id.Value, nameof(AppPlatformEnum.Bungie), cancellationToken).ConfigureAwait(false);
            
            if (thirdResult == null)
            {
                _logger.Warning("Not found third party data for user {UserId}", _currentUser.Id);
                goto Break;
            }
            
            _playerData.BungieMembershipId = thirdResult.ThirdPartyUserId;
            
            var playerUniqueName = await _cacheManager.Get<string>(
                $"{BungieIdCachePrefix}:{_playerData.BungieMembershipId}", cancellationToken).ConfigureAwait(false);
            if (!string.IsNullOrEmpty(playerUniqueName))
            {
                var playerCache = await _cacheManager.Get<Destiny2PlayerData>(
                    $"{PlayerCachePrefix}:{playerUniqueName}", cancellationToken).ConfigureAwait(false);;
                if (playerCache != null)
                {
                    isOk = true;
                    _playerData.AssignFrom(playerCache);
                    goto Break;
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
                goto Break;
            }
            
            var bungieNetUser = bungieNetUserResponse.Data.Response;
            
            _playerData.GlobalDisplayName = bungieNetUser.CachedBungieGlobalDisplayName;
            _playerData.GlobalDisplayNameCode = bungieNetUser.CachedBungieGlobalDisplayNameCode.ToString().PadLeft(4, '0');
            _playerData.FirstAccess = bungieNetUser.FirstAccess;
            _playerData.LastUpdate = bungieNetUser.LastUpdate;
            _playerData.ShowActivity = bungieNetUser.ShowActivity;
            _playerData.Language = bungieNetUser.Locale;
            
            isOk = await LoadPlayerProfilesAsync(BungieMembershipTypeEnum.None, cancellationToken);
            if (isOk)
            {
                await _cacheManager.Set($"{BungieIdCachePrefix}:{_playerData.BungieMembershipId}", _playerData.UniqueName, _expiryTime, cancellationToken).ConfigureAwait(false);
            }
        }

        Break:
        if (!isOk)
        {
            throw new ArgumentException("Not found destiny player data.");
        }

        await _cacheManager.Set(
            $"{PlayerCachePrefix}:{_playerData.UniqueName}", _playerData, _expiryTime, cancellationToken).ConfigureAwait(false);
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
        ExceptionDispatchInfo.Capture(ex).Throw();
        return Task.CompletedTask;
    }

    private async Task<bool> LoadPlayerProfilesAsync(BungieMembershipTypeEnum membershipType, CancellationToken cancellationToken)
    {
        var profileResult = await _bungieService.GetDestiny2ProfileListByBungieNameAsync(
            new GetDestiny2ProfileListByBungieNameRequest
            {
                DisplayName = _playerData.GlobalDisplayName,
                DisplayNameCode = _playerData.GlobalDisplayNameCode,
                MembershipType = BungieMembershipTypeEnum.All
            }, cancellationToken).ConfigureAwait(false);

        if (profileResult.Data == null || profileResult.Data.Response.Count == 0)
        {
            _logger.Debug("No Destiny 2 profiles found for {UniqueName}", _playerData.UniqueName);
            return false;
        }
        
        var profiles = profileResult.Data.Response;
        Destiny2PlayerByBungieNameDto? profile = null;
        
        if (profiles.Count == 1)
        {
            profile = profiles[0];
            
            _playerData.IsSinglePlatform = true;
            _playerData.Profiles = profiles.Select(x =>
                new Destiny2PlayerDataMembershipItem
                {
                    DisplayName = x.DisplayName,
                    MembershipType = x.MembershipType,
                    MembershipId = x.MembershipId
                }).ToList();
        }
        else
        {
            _playerData.OnCrossSave = profiles.All(x => x.CrossSaveOverride != BungieMembershipTypeEnum.None);
            switch (_playerData.OnCrossSave)
            {
                case true:
                {
                    var crossType = profiles.First().CrossSaveOverride;
                    profile = profiles.First(x => x.MembershipType == crossType);
                    break;
                }
                case false when membershipType != BungieMembershipTypeEnum.None:
                {
                    profile = profiles.FirstOrDefault(x => x.MembershipType == membershipType);
                    break;
                }
            }
        }

        if (profile == null)
        {
            _logger.Debug("No primary membership found for {UniqueName}", _playerData.UniqueName);
            return false;
        }
        
        _playerData.PrimaryMembershipType = profile.MembershipType;
        _playerData.PrimaryMembershipId = profile.MembershipId;
        
        return true;
    }
}