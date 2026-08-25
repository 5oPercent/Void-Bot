namespace Void.Bot.Api.OpenApi;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class OpenApiTagOrderAttribute(int order) : Attribute
{
    public int Order { get; } = order;
}
