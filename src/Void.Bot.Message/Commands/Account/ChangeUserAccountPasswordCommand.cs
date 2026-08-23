using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;

namespace Void.Bot.Message.Commands.Account;

public class ChangeUserAccountPasswordCommand : ICommand
{
    [JsonIgnore]
    public Guid? UserId { get; set; }
    
    /// <summary>
    /// 旧密码
    /// </summary>
    public required string OldPassword { get; set; }
    
    /// <summary>
    /// 新密码
    /// </summary>
    public required string NewPassword { get; set; }
    
    /// <summary>
    /// 新密码确认
    /// </summary>
    public required string NewPasswordConfirm { get; set; }

}

public class ChangeUserAccountPasswordResponse : VoidBotResponse
{
}