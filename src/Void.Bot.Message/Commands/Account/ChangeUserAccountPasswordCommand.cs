using System.Text.Json.Serialization;
using Mediator.Net.Contracts;
using Void.Bot.Message.Dto;
using Void.Bot.Message.Request.Identity;

namespace Void.Bot.Message.Commands.Account;

public class ChangeUserAccountPasswordCommand : ICommand, IRequireTwoFactorAuthentication
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
