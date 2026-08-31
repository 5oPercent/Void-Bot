using Void.Bot.Message.Attributes;

namespace Void.Bot.Core.Resources.i18n.Endpoint;

[Language("Endpoint", "ApiKey")]
public sealed class ApiKeyEndpointContextTemplate : IContextTemplate
{
    public string Create { get; set; } = "创建 API Key";
    public string Delete { get; set; } = "删除 API Key";

}
