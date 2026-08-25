using System.Reflection;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Void.Bot.Api.OpenApi;

public sealed class OpenApiTagOrderDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string ControllerSuffix = "Controller";
    private const string OrderExtension = "x-order";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        if (document.Tags == null)
            return Task.CompletedTask;

        var controllerOrders = typeof(OpenApiTagOrderDocumentTransformer).Assembly.GetTypes()
            .Where(type => typeof(ControllerBase).IsAssignableFrom(type) &&
                           type.Name.EndsWith(ControllerSuffix, StringComparison.Ordinal))
            .Select(type => new
            {
                Name = type.Name[..^ControllerSuffix.Length],
                Order = type.GetCustomAttribute<OpenApiTagOrderAttribute>()?.Order
            })
            .Where(controller => controller.Order.HasValue)
            .ToDictionary(controller => controller.Name, controller => controller.Order.GetValueOrDefault());

        foreach (var tag in document.Tags)
        {
            if (tag.Name == null || !controllerOrders.TryGetValue(tag.Name, out var order))
                continue;

            tag.Extensions ??= new Dictionary<string, IOpenApiExtension>();
            tag.Extensions[OrderExtension] = new JsonNodeExtension(JsonValue.Create(order));
        }

        return Task.CompletedTask;
    }
}
