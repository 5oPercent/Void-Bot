using System.Net;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto.Identity;
using Void.Bot.Message.Enum;
using Void.Bot.Message.Enum.Bungie;
using Void.Bot.Message.Enum.Bungie.Destiny;
using Void.Bot.Message.Request.Bungie;
using Void.Bot.Message.Request.Bungie.Domain;

namespace Void.Bot.Core.Services.Bungie;

public partial interface IBungieService : IScope
{
    public Task<GetDestinyLinkedProfileResponse> GetDestinyLinkedProfileAsync(
        GetDestinyLinkedProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestinyProfileListByBungieNameResponse> GetDestinyProfileListByBungieNameAsync(
        GetDestinyProfileListByBungieNameRequest request, CancellationToken cancellationToken);

    public Task<GetDestinyBaseProfileResponse> GetDestinyBaseProfileAsync(
        GetDestinyBaseProfileRequest request, CancellationToken cancellationToken);
    
    public Task<GetDestinyCareerResponse> GetDestinyCareerAsync(
        GetDestinyCareerRequest request, CancellationToken cancellationToken);
}

public partial class BungieService : IBungieService
{
    public async Task<GetDestinyLinkedProfileResponse> GetDestinyLinkedProfileAsync(
        GetDestinyLinkedProfileRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetLinkedProfilesAsync(
            request.MembershipType, request.MembershipId, cancellationToken).ConfigureAwait(false);

        return new GetDestinyLinkedProfileResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestinyProfileListByBungieNameResponse> GetDestinyProfileListByBungieNameAsync(
        GetDestinyProfileListByBungieNameRequest request, CancellationToken cancellationToken)
    {
        var name = request.FindBungieName.Split("#")[0];
        var code = int.Parse(request.FindBungieName.Split("#")[1]);
        
        var result = await _dataProvider.SearchDestinyPlayerByBungieNameAsync(
            request.MembershipType, name, code, cancellationToken).ConfigureAwait(false);

        return new GetDestinyProfileListByBungieNameResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
    
    public async Task<GetDestinyBaseProfileResponse> GetDestinyBaseProfileAsync(
        GetDestinyBaseProfileRequest request, CancellationToken cancellationToken)
    {
        var findBungieName = request.FindBungieName;
        var findMembershipType = request.MembershipType;

        var response = new GetDestinyBaseProfileResponse();
        
        var isSinglePlatform = false;                       // 单个游玩平台信息
        var onCrossSave = false;                            // 是否启用跨平台
        var membershipType = BungieMembershipType.None;     // 游玩平台类型
        long membershipId = 0;                              // 游玩平台账号ID
        
        if (string.IsNullOrEmpty(findBungieName))
        {
            if (_currentUser.Id == null || _currentUser.Id == Guid.Empty)
            {
                response.Message = $"请先绑定{AppPlatformEnum.Void}账号.";
                return response;
            }
            
            var bungieInfo = await _accountDataProvider.GetThirdPartyByUserIdAsync(
                _currentUser.Id.Value, AppPlatformEnum.Bungie.ToString(), cancellationToken).ConfigureAwait(false);
            
            if (bungieInfo == null)
            {
                response.Message = $"请先绑定{AppPlatformEnum.Bungie}账号.";
                return response;
            }
            
            var playerInfo = bungieInfo.ExtraData.DeserializeJson<ThirdPartyBungieExtraDataDto>();
            
            onCrossSave = playerInfo.Profiles.Any(x => x.IsCrossSavePrimary);

            if (onCrossSave)
            {
                var profile = playerInfo.Profiles.First(x => x.IsCrossSavePrimary);

                findBungieName = $"{profile.BungieGlobalDisplayName}#{profile.BungieGlobalDisplayNameCode}";
                membershipType = profile.CrossSaveOverride;
                membershipId = long.Parse(profile.MembershipId);
            }
            else if (findMembershipType != null)
            {
                var profile = playerInfo.Profiles.FirstOrDefault(x => x.MembershipType == findMembershipType);
                
                if (profile == null)
                {
                    response.Message = $"当前账号不存在{findMembershipType}游玩平台信息. 如果已存在, 请重新绑定{AppPlatformEnum.Bungie}更新数据.";
                    return response;
                }
                
                findBungieName = $"{profile.BungieGlobalDisplayName}#{profile.BungieGlobalDisplayNameCode}";
                membershipType = profile.MembershipType;
                membershipId = long.Parse(profile.MembershipId);
            }
        }
        else
        {
            var searchResult = await GetDestinyProfileListByBungieNameAsync(
                new GetDestinyProfileListByBungieNameRequest
                {
                    FindBungieName = findBungieName,
                    MembershipType = findMembershipType ?? BungieMembershipType.All
                }, cancellationToken).ConfigureAwait(false);
            
            if (searchResult.Data == null || searchResult.Data.Response.Count == 0)
            {
                response.Message = $"不存在游玩平台信息, 请检查输入是否正确.";
                return response;
            }
            
            BungieApiSearchDestinyPlayerByBungieNameDto? profile = null;

            isSinglePlatform = searchResult.Data.Response.Count == 1;
            
            if (isSinglePlatform)
            {
                profile = searchResult.Data.Response.First();
            }
            else
            {
                onCrossSave = searchResult.Data.Response.All(x => x.CrossSaveOverride != BungieMembershipType.None);
                switch (onCrossSave)
                {
                    case true:
                    {
                        var crossType = searchResult.Data.Response.First().CrossSaveOverride;
                        profile = searchResult.Data.Response.First(x => x.MembershipType == crossType);
                        break;
                    }
                    case false when findMembershipType != BungieMembershipType.None:
                        profile = searchResult.Data.Response.FirstOrDefault(x => x.MembershipType == findMembershipType);
                        break;
                }
            }
            
            if (profile != null)
            {
                membershipType = profile.MembershipType;
                membershipId = long.Parse(profile.MembershipId);
            }
        }
        
        response.Data = new DestinyBaseProfileDto
        {
            DisplayName = findBungieName,
            OnCrossSave = onCrossSave,
            IsSinglePlatform = isSinglePlatform,
            MembershipType = membershipType,
            MembershipId = membershipId
        };

        return response;
    }

    public async Task<GetDestinyCareerResponse> GetDestinyCareerAsync(
        GetDestinyCareerRequest request, CancellationToken cancellationToken)
    {
        var result = await _dataProvider.GetProfileAsync(
            request.MembershipType, request.MembershipId,
            [DestinyComponentTypeEnum.Profiles, DestinyComponentTypeEnum.SocialCommendations],
            cancellationToken).ConfigureAwait(false);

        return new GetDestinyCareerResponse
        {
            Code = result != null ? HttpStatusCode.OK : HttpStatusCode.InternalServerError,
            Data = result
        };
    }
}