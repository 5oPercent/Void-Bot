using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "Identity")]
public sealed class IdentityEndpointContextTemplate : IContextTemplate
{
    public string RegisterUserAccount { get; set; } = "注册用户账户";
    public string SendRegistrationEmailCode { get; set; } = "发送注册邮件验证码";
    public string GetEmailStatus { get; set; } = "获取邮件状态";
    public string SendEmailBindingCode { get; set; } = "发送邮箱绑定验证码";
    public string BindEmail { get; set; } = "绑定邮箱";
    public string SendEmailUnbindingCode { get; set; } = "发送邮箱解绑验证码";
    public string UnbindEmail { get; set; } = "解绑邮箱";
    public string SendPasswordResetCode { get; set; } = "发送密码重置验证码";
    public string ResetPassword { get; set; } = "重置密码";
    public string ResetUserAccountPasswordByLink { get; set; } = "通过链接重置用户账户密码";
    public string Login { get; set; } = "用户登录";
    public string RefreshToken { get; set; } = "刷新令牌";
    public string GetCurrentUserAccount { get; set; } = "获取当前用户账户";
    public string ChangePassword { get; set; } = "修改密码";
    public string CanChangeName { get; set; } = "检查是否可以修改用户名";
    public string ChangeName { get; set; } = "修改用户名";
    public string GenerateAuthenticateUrlFromSteam { get; set; } = "生成 Steam 认证地址";
    public string LoginSteam { get; set; } = "使用 Steam 登录";

}
