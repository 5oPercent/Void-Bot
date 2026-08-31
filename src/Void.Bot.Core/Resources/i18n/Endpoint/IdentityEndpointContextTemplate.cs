using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Identity")]
public sealed class IdentityEndpointContextTemplate : IContextTemplate
{
    public string RegisterUserAccount { get; set; } = "注册用户账户";
    public string SendRegistrationEmailCodeAsync { get; set; } = "发送注册邮件验证码";
    public string GetEmailStatusAsync { get; set; } = "获取邮件状态";
    public string SendEmailBindingCodeAsync { get; set; } = "发送邮箱绑定验证码";
    public string BindEmailAsync { get; set; } = "绑定邮箱";
    public string SendEmailUnbindingCodeAsync { get; set; } = "发送邮箱解绑验证码";
    public string UnbindEmailAsync { get; set; } = "解绑邮箱";
    public string SendPasswordResetCodeAsync { get; set; } = "发送密码重置验证码";
    public string ResetPasswordAsync { get; set; } = "重置密码";
    public string ResetUserAccountPasswordByLinkAsync { get; set; } = "通过链接重置用户账户密码";
    public string LoginAsync { get; set; } = "用户登录";
    public string RefreshTokenAsync { get; set; } = "刷新令牌";
    public string GetCurrentUserAccountAsync { get; set; } = "获取当前用户账户";
    public string ChangePasswordAsync { get; set; } = "修改密码";
    public string CanChangeNameAsync { get; set; } = "检查是否可以修改用户名";
    public string ChangeNameAsync { get; set; } = "修改用户名";
    public string GenerateAuthenticateUrlFromSteam { get; set; } = "生成 Steam 认证地址";
    public string LoginSteamAsync { get; set; } = "使用 Steam 登录";

}
