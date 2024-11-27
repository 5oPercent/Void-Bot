using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Dto.Authorization;

namespace Void.Bot.Message.Commands.Account;

public class ChangeUserAccountPasswordCommand : ICommand
{
	public Guid? UserId { get; set; }
	/// <summary>
	/// 旧密码
	/// </summary>
	public string OldPassword { get; set; }
	/// <summary>
	/// 新密码
	/// </summary>
	public string NewPassword { get; set; }
	/// <summary>
	/// 新密码确认
	/// </summary>
	public string NewPasswordConfirm { get; set; }

}

public class ChangeUserAccountPasswordResponse : VoidBotResponse
{
}