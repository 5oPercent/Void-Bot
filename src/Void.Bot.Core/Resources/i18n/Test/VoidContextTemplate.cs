using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Test;

[Language("Void", "Void")]
public class VoidContextTemplate : IContextTemplate
{
    public string Void { get; set; } = "你好，欢迎使用我们的服务！";
}