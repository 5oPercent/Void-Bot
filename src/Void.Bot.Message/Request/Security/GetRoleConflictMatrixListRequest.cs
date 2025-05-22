using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetRoleConflictMatrixListRequest : IRequest
{
    [Required]
    [MinLength(1)]
    public List<Guid> RoleIdList { get; set; } = [];
}

public class GetRoleConflictMatrixListResponse : VoidBotResponse<List<RoleConflictMatrixDto>>
{
}