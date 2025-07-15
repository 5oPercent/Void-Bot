using System.Net;
using System.Text.RegularExpressions;
using Mediator.Net.Context;
using Mediator.Net.Contracts;
using Mediator.Net.Pipeline;
using Void.Bot.Core.Services.Account;
using Void.Bot.Core.Services.Bungie;
using Void.Bot.Core.Services.Identity;
using Void.Bot.Message.Attributes;
using Void.Bot.Message.Constants.Bungie;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Middlewares.Preload;

public partial class LoadDestiny2PlayerSpecification<TContext> : IPipeSpecification<TContext>
    where TContext : IContext<IMessage>
{
    private bool _isGetCurrentPlayerData = false;
    private bool _isGetPlayerDataByNameCode = false;

    private readonly ICurrentUser _currentUser;
    private Destiny2PlayerData _playerData;
    private readonly IBungieService _bungieService;
    private readonly IAccountDataProvider _accountDataProvider;

    public LoadDestiny2PlayerSpecification(
        ICurrentUser currentUser,
        Destiny2PlayerData playerData,
        IBungieService bungieService,
        IAccountDataProvider accountDataProvider)
    {
        _currentUser = currentUser;
        _playerData = playerData;
        _bungieService = bungieService;
        _accountDataProvider = accountDataProvider;
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
            
            if (UniqueNameRegex().IsMatch(_playerData.UniqueName))
            {
                var result = await _bungieService.GetDestiny2ProfileListByBungieNameAsync(
                    new GetDestiny2ProfileListByBungieNameRequest
                    {
                        DisplayName = _playerData.GlobalDisplayName,
                        DisplayNameCode = _playerData.GlobalDisplayNameCode,
                        MembershipType = request.MembershipType
                    }, cancellationToken).ConfigureAwait(false);

                if (result.Data != null && result.Data.Response.Count != 0)
                {
                    var profiles = result.Data.Response;
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
                            case false when request.MembershipType != BungieMembershipTypeEnum.None:
                            {
                                profile = profiles.FirstOrDefault(x => x.MembershipType == request.MembershipType);
                                break;
                            }
                        }
                    }
                        
                    if (profile != null)
                    {
                        _playerData.PrimaryMembershipType = profile.MembershipType;
                        _playerData.PrimaryMembershipId = profile.MembershipId;
                        
                        // TODO 补全注册时间、最后更新时间、是否允许展示活动状态、语言
                    }
                    
                    isOk = true;
                }
            }
        }
        else if (_isGetCurrentPlayerData && _currentUser.Id.HasValue) // 根据绑定的信息查询玩家数据.
        {
            var result = await _accountDataProvider.GetThirdPartyByUserIdAsync(
                _currentUser.Id.Value, nameof(AppPlatformEnum.Bungie), cancellationToken).ConfigureAwait(false);

            if (result != null)
            {
                _playerData.BungieMembershipId = result.ThirdPartyUserId;
                
                // TODO 补充 playerdata
                
                isOk = true;
            }
        }

        if (!isOk)
        {
            throw new ArgumentException("Not found destiny player data.");
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
        if (!ShouldExecute(context, default) ||  context.Result is not VoidBotResponse) throw ex;
        
        var response = (dynamic)context.Result;

        response.Code = HttpStatusCode.InternalServerError;
        response.Message = ex.Message;

        throw ex;
    }

    [GeneratedRegex(BungieConstants.IdPattern)]
    private static partial Regex UniqueNameRegex();
}