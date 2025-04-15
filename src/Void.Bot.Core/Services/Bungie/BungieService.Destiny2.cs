using System.Net;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny2;
using Void.Bot.Message.Request.Account;
using Void.Bot.Message.Request.Bungie.Destiny2;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    public Task<GetDestiny2LinkedProfileResponse> GetDestiny2LinkedProfileAsync(
        GetDestiny2LinkedProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestiny2ProfileListByBungieNameResponse> GetDestiny2ProfileListByBungieNameAsync(
        GetDestiny2ProfileListByBungieNameRequest request, CancellationToken cancellationToken);

    public Task<GetDestiny2BaseProfileResponse> GetDestiny2BaseProfileAsync(
        GetDestiny2BaseProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestiny2CareerResponse> GetDestiny2CareerAsync(
        GetDestiny2CareerRequest request, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    public async Task<GetDestiny2LinkedProfileResponse> GetDestiny2LinkedProfileAsync(
        GetDestiny2LinkedProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2LinkedProfilesAsync(
            request.MembershipTypeEnum, request.MembershipId, cancellationToken).ConfigureAwait(false);

        return new GetDestiny2LinkedProfileResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestiny2ProfileListByBungieNameResponse> GetDestiny2ProfileListByBungieNameAsync(
        GetDestiny2ProfileListByBungieNameRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2PlayerByBungieNameAsync(
            request.MembershipTypeEnum, request.DisplayName, request.DisplayNameCode, cancellationToken).ConfigureAwait(false);

        return new GetDestiny2ProfileListByBungieNameResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestiny2BaseProfileResponse> GetDestiny2BaseProfileAsync(
        GetDestiny2BaseProfileRequest request, CancellationToken cancellationToken)
    {
        var triggerPlatform = request.TriggerPlatform;
        var triggerPlatformUserId = request.TriggerPlatformUserId;
        
        var findBungieName = request.FindBungieName;
        var findMembershipType = request.MembershipType;

        var response = new GetDestiny2BaseProfileResponse();
        
        var isSinglePlatform = false;                       // 单个游玩平台信息
        var onCrossSave = false;                            // 是否启用跨平台
        var membershipType = BungieMembershipTypeEnum.None;     // 游玩平台类型
        long membershipId = 0;                              // 游玩平台账号ID
        
        if (string.IsNullOrEmpty(findBungieName))
        {
            if (triggerPlatform == null || string.IsNullOrEmpty(triggerPlatformUserId))
            {
                response.Message = $"请提供触发平台的账号信息.";
                return response;
            }

            ThirdPartyBungieExtraDataDto? playerInfo;
            
            if (triggerPlatform == AppPlatformEnum.Void)
            {
                var isOk = Guid.TryParse(triggerPlatformUserId, out var userId);

                if (!isOk)
                {
                    response.Message = $"请提供正确的{AppPlatformEnum.Void}账号信息.";
                    return response;
                }

                var bungieInfo = await _accountDataProvider.GetThirdPartyByUserIdAsync(userId, AppPlatformEnum.Bungie.ToString(), cancellationToken).ConfigureAwait(false);
                
                if (bungieInfo == null)
                {
                    response.Message = $"请先绑定{AppPlatformEnum.Bungie}账号.";
                    return response;
                }
                
                playerInfo = bungieInfo.ExtraData.DeserializeJson<ThirdPartyBungieExtraDataDto>();
            }
            else
            {
                var currentLinkBungieInfo = await _accountService.GetThirdPartyByThirdPartyAsync(
                    new GetThirdPartyByThirdPartyRequest
                    {
                        CurrentPlatform = triggerPlatform.Value,
                        ThirdPartyUserId = triggerPlatformUserId,
                        TargetPlatform = AppPlatformEnum.Bungie
                    }, cancellationToken).ConfigureAwait(false);

                if (currentLinkBungieInfo.Data == null)
                {
                    response.Message = $"请先绑定{AppPlatformEnum.Bungie}账号.";
                    return response;
                }

                playerInfo = currentLinkBungieInfo.Data.ExtraData.DeserializeJson<ThirdPartyBungieExtraDataDto>();
            }
            
            onCrossSave = playerInfo.Profiles.Any(x => x.IsCrossSavePrimary);

            if (onCrossSave)
            {
                var profile = playerInfo.Profiles.First(x => x.IsCrossSavePrimary);
                
                membershipType = profile.CrossSaveOverride;
                membershipId = long.Parse(profile.MembershipId);
            }
            else if (findMembershipType != null)
            {
                var profile = playerInfo.Profiles.FirstOrDefault(x => x.MembershipTypeEnum == findMembershipType);
                
                if (profile == null)
                {
                    response.Message = $"当前账号不存在{findMembershipType}游玩平台信息. 如果已存在, 请重新绑定{AppPlatformEnum.Bungie}更新数据.";
                    return response;
                }
                
                membershipType = profile.MembershipTypeEnum;
                membershipId = long.Parse(profile.MembershipId);
            }
        }
        else
        {
            var searchResult = await GetDestiny2ProfileListByBungieNameAsync(
                new GetDestiny2ProfileListByBungieNameRequest
                {
                    DisplayName = findBungieName.Split("#")[0],
                    DisplayNameCode = int.Parse(findBungieName.Split("#")[1]),
                    MembershipTypeEnum = findMembershipType ?? BungieMembershipTypeEnum.All
                }, cancellationToken).ConfigureAwait(false);
            
            if (searchResult.Data == null || searchResult.Data.Response.Count == 0)
            {
                response.Message = $"不存在游玩平台信息, 请检查输入是否正确.";
                return response;
            }
            
            Destiny2PlayerByBungieNameDto? profile = null;

            isSinglePlatform = searchResult.Data.Response.Count == 1;
            
            if (isSinglePlatform)
            {
                profile = searchResult.Data.Response.First();
            }
            else
            {
                onCrossSave = searchResult.Data.Response.All(x => x.CrossSaveOverride != BungieMembershipTypeEnum.None);
                switch (onCrossSave)
                {
                    case true:
                    {
                        var crossType = searchResult.Data.Response.First().CrossSaveOverride;
                        profile = searchResult.Data.Response.First(x => x.MembershipTypeEnum == crossType);
                        break;
                    }
                    case false when findMembershipType != BungieMembershipTypeEnum.None:
                        profile = searchResult.Data.Response.FirstOrDefault(x => x.MembershipTypeEnum == findMembershipType);
                        break;
                }
            }
            
            if (profile != null)
            {
                membershipType = profile.MembershipTypeEnum;
                membershipId = long.Parse(profile.MembershipId);
            }
        }
        
        response.Data = new Destiny2BaseProfileDto
        {
            OnCrossSave = onCrossSave,
            IsSinglePlatform = isSinglePlatform,
            MembershipTypeEnum = membershipType,
            MembershipId = membershipId
        };

        return response;
    }

    public async Task<GetDestiny2CareerResponse> GetDestiny2CareerAsync(
        GetDestiny2CareerRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetDestiny2ProfileAsync(
            request.MembershipTypeEnum, request.MembershipId,
            [Destiny2ComponentTypeEnum.Profiles, Destiny2ComponentTypeEnum.SocialCommendations],
            cancellationToken).ConfigureAwait(false);

        return new GetDestiny2CareerResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
}