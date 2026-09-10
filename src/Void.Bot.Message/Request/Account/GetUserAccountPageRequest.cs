using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Account;

namespace Void.Bot.Message.Request.Account;

public class GetUserAccountPageRequest : PageInputDto, IRequest
{
    public List<Guid> UserIds { get; set; } = [];
}

public class GetUserAccountPageResponse : VoidBotResponse<PagedListDto<UserAccountSecurityInfoDto>>
{
}
