using AutoMapper;
using Void.Bot.Core.Domain.Account;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Core.Mappings;

public class AccountMapping : Profile
{
    public AccountMapping()
    {
        CreateMap<UserAccount, UserAccountDto>().ReverseMap();
        CreateMap<UserAccount, UserAccountInfoDto>().ReverseMap();
        
        CreateMap<UserThirdParty, UserThirdPartyDto>().ReverseMap();
        CreateMap<UserThirdParty, UserThirdPartyInfoDto>().ReverseMap();
    }
}