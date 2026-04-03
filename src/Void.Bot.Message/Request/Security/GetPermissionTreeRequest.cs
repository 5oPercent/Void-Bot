using System.ComponentModel.DataAnnotations;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Security;

namespace Void.Bot.Message.Request.Security;

public class GetPermissionTreeRequest : IRequest
{
}

public class GetPermissionTreeResponse : VoidBotResponse<List<PermissionDto>>
{
}