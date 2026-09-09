using AutoMapper;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Core.Extension;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Mappings;

public class AccountMapping : Profile
{
    public AccountMapping()
    {
        CreateMap<UserAccount, UserAccountDto>().ReverseMap();
        CreateMap<UserAccount, UserAccountInfoDto>()
            .ForMember(x => x.IsTwoFactorEnabled, options =>
                options.MapFrom(x => x.TwoFactorEnabledAt.HasValue));
        CreateMap<UserAccountDto, UserAccountInfoDto>()
            .ForMember(x => x.IsTwoFactorEnabled, options =>
                options.MapFrom(x => x.TwoFactorEnabledAt.HasValue));
        
        CreateMap<UserThirdParty, UserThirdPartyDto>().ReverseMap();
        CreateMap<UserThirdParty, UserThirdPartyInfoDto>().ReverseMap();
        CreateMap<UserThirdPartyDto, UserThirdPartyInfoDto>().ReverseMap();

        CreateMap<PagedList<UserAccount>, PagedListDto<UserAccountDto>>().ReverseMap();
        CreateMap<PagedListDto<UserAccountDto>, PagedListDto<UserAccountInfoDto>>().ReverseMap();
    }
}
